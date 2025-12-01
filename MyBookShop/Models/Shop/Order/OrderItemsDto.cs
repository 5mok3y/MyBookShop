namespace MyBookShop.Models.Shop.Order
{
    public class OrderItemsDto
    {
        public required int BookId { get; set; }
        public required string BookTitle { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
    }
}
