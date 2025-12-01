using MyBookShop.Models.Enums;

namespace MyBookShop.Models.Shop.CheckOut
{
    public class CheckoutResultDto
    {
        public required string OrderId { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required int TotalPrice { get; set; }
        public required OrderStatus Status { get; set; }
        public required string RefId { get; set; }
    }
}
