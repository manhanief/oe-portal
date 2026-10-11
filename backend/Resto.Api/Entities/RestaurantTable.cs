namespace Resto.Api.Entities;

public class RestaurantTable
{
    public int TableId { get; set; }
    public string TableNo { get; set; } = string.Empty;
    public int Seats { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
