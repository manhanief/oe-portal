namespace Resto.Api.Entities;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }         // price at the time of ordering
    public string? Notes { get; set; }

    public Order Order { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
}
