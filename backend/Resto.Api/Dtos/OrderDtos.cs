using Resto.Api.Entities;

namespace Resto.Api.Dtos;

// ---------- Requests (sent BY the frontend) ----------

/// <summary>Note: no prices here. The server looks up real prices from the database.</summary>
public record CreateOrderItemRequest(
    int ItemId,
    int Quantity,
    string? Notes);

public record CreateOrderRequest(
    OrderType OrderType,
    int? TableId,
    string? CustomerName,
    List<CreateOrderItemRequest> Items);

public record UpdateOrderStatusRequest(OrderStatus Status);

// ---------- Responses (sent TO the frontend) ----------

public record OrderItemDto(
    int ItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? Notes);

public record OrderDto(
    int Id,
    string OrderNo,
    OrderType OrderType,
    string? TableNo,
    string? CustomerName,
    OrderStatus Status,
    decimal Subtotal,
    decimal Tax,
    decimal Total,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items);
