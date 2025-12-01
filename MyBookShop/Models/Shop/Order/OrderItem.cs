using MyBookShop.Models.Library.Books;

namespace MyBookShop.Models.Shop.Order
{
    public class OrderItem
    {
        public Guid id { get; set; }
        public Guid OrderId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public Book Book { get; set; } = null!;

        public int GetTotalPrice() => UnitPrice * Quantity;
    }
}
