using Resto.Api.Dtos;
using Resto.Api.Entities;

namespace Resto.Api.Services;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct);

    Task<OrderDto> GetAsync(int orderId, CancellationToken ct);

    /// <summary>status = null returns all active orders (not completed or cancelled).</summary>
    Task<IReadOnlyList<OrderDto>> ListAsync(OrderStatus? status, CancellationToken ct);

    Task<OrderDto> UpdateStatusAsync(int orderId, OrderStatus newStatus, CancellationToken ct);
}
