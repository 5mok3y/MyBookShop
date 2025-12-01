using MyBookShop.Models.Enums;
using MyBookShop.Models.Identity;

namespace MyBookShop.Models.Shop.Order
{
    public class Order
    {
        public Guid Id { get; set; }
        public required string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int TotalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        // (Pending, Paid, Shipped, Delivered, Canceled)
        public string? CardPan { get; set; }
        public required string RefId { get; set; }

        // Navigation
        public MyApplicationUser User { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
