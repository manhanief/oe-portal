using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resto.Api.Auth;
using Resto.Api.Dtos;
using Resto.Api.Entities;
using Resto.Api.Services;

namespace Resto.Api.Controllers;

/// <summary>
/// Thin controller: receives the request and hands it to OrderService.
/// Input validation runs automatically (ValidationFilter); business rules live in the service.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orders) : ControllerBase
{
    /// <summary>Customer places an order. No login needed.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var order = await orders.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Customer tracks an order. No login needed.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken ct) =>
        Ok(await orders.GetAsync(id, ct));

    /// <summary>
    /// Kitchen board (staff only). Without ?status, returns all active orders, oldest first.
    /// Example: /api/orders?status=Pending
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthPolicies.Staff)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> List(
        [FromQuery] OrderStatus? status, CancellationToken ct) =>
        Ok(await orders.ListAsync(status, ct));

    /// <summary>Kitchen moves an order to the next status (staff only).</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = AuthPolicies.Staff)]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        int id, UpdateOrderStatusRequest request, CancellationToken ct) =>
        Ok(await orders.UpdateStatusAsync(id, request.Status, ct));
}
