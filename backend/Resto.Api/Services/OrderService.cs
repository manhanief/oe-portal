using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using Resto.Api.Common;
using Resto.Api.Data;
using Resto.Api.Dtos;
using Resto.Api.Entities;

namespace Resto.Api.Services;

public class OrderService(
    RestoDbContext db,
    OrderNumberGenerator orderNumbers,
    IOptions<PricingOptions> pricing,
    ILogger<OrderService> logger) : IOrderService
{
    // Which status changes are allowed. Anything not listed is rejected.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Pending]   = [OrderStatus.Preparing, OrderStatus.Cancelled],
        [OrderStatus.Preparing] = [OrderStatus.Ready, OrderStatus.Cancelled],
        [OrderStatus.Ready]     = [OrderStatus.Completed],
        [OrderStatus.Completed] = [],
        [OrderStatus.Cancelled] = []
    };

    private const int MaxOrderNumberAttempts = 3;

    // ---------------------------------------------------------------
    // Create
    // ---------------------------------------------------------------
    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct)
    {
        // 1. Dine-in table must exist
        if (request.OrderType == OrderType.DineIn)
        {
            var tableExists = await db.RestaurantTables.AnyAsync(t => t.TableId == request.TableId, ct);
            if (!tableExists)
                throw new BusinessRuleException($"Table {request.TableId} does not exist.");
        }

        // 2. Load the real menu items from the database (never trust prices from the browser)
        var itemIds = request.Items.Select(i => i.ItemId).Distinct().ToList();

        var menuItems = await db.MenuItems
            .Where(m => itemIds.Contains(m.ItemId))
            .ToDictionaryAsync(m => m.ItemId, ct);

        // 3. Every item must exist and be available
        foreach (var id in itemIds)
        {
            if (!menuItems.TryGetValue(id, out var menuItem))
                throw new BusinessRuleException($"Menu item {id} does not exist.");

            if (!menuItem.IsAvailable)
                throw new BusinessRuleException($"{menuItem.Name} is currently unavailable.");
        }

        // 4. Build the order lines, copying today's price into UNIT_PRICE
        var order = new Order
        {
            OrderType = request.OrderType,
            TableId = request.OrderType == OrderType.DineIn ? request.TableId : null,
            CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? null : request.CustomerName.Trim(),
            Status = OrderStatus.Pending,
            Items = request.Items.Select(i => new OrderItem
            {
                ItemId = i.ItemId,
                Quantity = i.Quantity,
                UnitPrice = menuItems[i.ItemId].Price,
                Notes = string.IsNullOrWhiteSpace(i.Notes) ? null : i.Notes.Trim()
            }).ToList()
        };

        // 5. Totals
        order.Subtotal = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        order.Tax = Math.Round(order.Subtotal * pricing.Value.TaxRate, 2, MidpointRounding.AwayFromZero);
        order.Total = order.Subtotal + order.Tax;

        // 6. Save. SaveChanges inserts the order AND its items in ONE transaction:
        //    if any insert fails, nothing is saved.
        //    If two orders get the same number at the same moment, the unique index
        //    rejects one of them and we simply retry with the next number.
        for (var attempt = 1; ; attempt++)
        {
            order.OrderNo = await orderNumbers.NextAsync(ct);
            db.Orders.Add(order);

            try
            {
                await db.SaveChangesAsync(ct);
                break;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex) && attempt < MaxOrderNumberAttempts)
            {
                logger.LogWarning("Order number {OrderNo} was taken, retrying (attempt {Attempt})",
                    order.OrderNo, attempt);
            }
        }

        logger.LogInformation("Order {OrderNo} created, total {Total}", order.OrderNo, order.Total);

        return await GetAsync(order.OrderId, ct);
    }

    // ---------------------------------------------------------------
    // Read
    // ---------------------------------------------------------------
    public async Task<OrderDto> GetAsync(int orderId, CancellationToken ct)
    {
        var order = await OrdersWithDetails()
            .FirstOrDefaultAsync(o => o.OrderId == orderId, ct)
            ?? throw new NotFoundException($"Order {orderId} was not found.");

        return ToDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> ListAsync(OrderStatus? status, CancellationToken ct)
    {
        var query = OrdersWithDetails();

        query = status is null
            ? query.Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            : query.Where(o => o.Status == status);

        var orders = await query
            .OrderBy(o => o.CreatedAt)   // oldest first, so the kitchen works in order
            .ToListAsync(ct);

        return orders.Select(ToDto).ToList();
    }

    // ---------------------------------------------------------------
    // Update status
    // ---------------------------------------------------------------
    public async Task<OrderDto> UpdateStatusAsync(int orderId, OrderStatus newStatus, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId, ct)
            ?? throw new NotFoundException($"Order {orderId} was not found.");

        if (!AllowedTransitions[order.Status].Contains(newStatus))
            throw new BusinessRuleException(
                $"Order {order.OrderNo} cannot change from {order.Status} to {newStatus}.");

        logger.LogInformation("Order {OrderNo}: {From} -> {To}", order.OrderNo, order.Status, newStatus);

        order.Status = newStatus;
        await db.SaveChangesAsync(ct);

        return await GetAsync(orderId, ct);
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------
    private IQueryable<Order> OrdersWithDetails() =>
        db.Orders
            .AsNoTracking()
            .Include(o => o.Table)
            .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem);

    private static OrderDto ToDto(Order o) => new(
        o.OrderId,
        o.OrderNo,
        o.OrderType,
        o.Table?.TableNo,
        o.CustomerName,
        o.Status,
        o.Subtotal,
        o.Tax,
        o.Total,
        o.CreatedAt,
        o.Items
            .OrderBy(i => i.OrderItemId)
            .Select(i => new OrderItemDto(
                i.ItemId,
                i.MenuItem.Name,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity,
                i.Notes))
            .ToList());

    /// <summary>ORA-00001 = unique constraint violated</summary>
    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is OracleException { Number: 1 };
}
