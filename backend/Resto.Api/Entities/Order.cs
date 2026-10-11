namespace Resto.Api.Entities;

public class Order
{
    public int OrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public int? TableId { get; set; }              // null for takeaway
    public OrderType OrderType { get; set; }
    public string? CustomerName { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }        // filled by Oracle (SYSTIMESTAMP)

    public RestaurantTable? Table { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
