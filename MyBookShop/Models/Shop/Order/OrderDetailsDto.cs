using MyBookShop.Models.Enums;

namespace MyBookShop.Models.Shop.Order
{
    public class OrderDetailsDto
    {
        public required string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public required string RefId { get; set; }
        public required List<OrderItemsDto> OrderItems { get; set; } = new();
    }
}
