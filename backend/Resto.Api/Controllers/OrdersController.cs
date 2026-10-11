using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Resto.Api.Dtos;
using Resto.Api.Entities;
using Resto.Api.Services;

namespace Resto.Api.Controllers;

/// <summary>
/// Thin controller: receives the request, validates it, and hands it to OrderService.
/// All business rules live in the service.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController(
    IOrderService orders,
    IValidator<CreateOrderRequest> createValidator) : ControllerBase
{
    /// <summary>Customer places an order.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        var order = await orders.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Customer tracks an order; kitchen views order details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken ct) =>
        Ok(await orders.GetAsync(id, ct));

    /// <summary>
    /// Kitchen board. Without ?status, returns all active orders (oldest first).
    /// Example: /api/orders?status=Pending
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> List(
        [FromQuery] OrderStatus? status, CancellationToken ct) =>
        Ok(await orders.ListAsync(status, ct));

    /// <summary>Kitchen moves an order to the next status.</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        int id, UpdateOrderStatusRequest request, CancellationToken ct) =>
        Ok(await orders.UpdateStatusAsync(id, request.Status, ct));
}
