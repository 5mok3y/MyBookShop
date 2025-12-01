using MyBookShop.Models.Library.Books;

namespace MyBookShop.Models.Shop.Cart
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }

        // Navigation
        public Cart Cart { get; set; } = null!;
        public Book Book { get; set; } = null!;

        public int GetTotalPrice() => UnitPrice * Quantity;
    }
}
