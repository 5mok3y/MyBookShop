using MyBookShop.Models.Identity;
using MyBookShop.Models.Payment;

namespace MyBookShop.Models.Shop.Cart
{
    public class Cart
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsCheckedOut { get; set; } = false;
        public DateTime? CheckedOutDate { get; set; }

        // Navigation
        public MyApplicationUser User { get; set; } = null!;
        public List<CartItem> CartItems { get; set; } = new();
        public List<Transaction> Payments { get; set; } = new();
    }
}
