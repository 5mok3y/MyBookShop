using MyBookShop.Models.Enums;
using MyBookShop.Models.Identity;
using MyBookShop.Models.Shop.Cart;

namespace MyBookShop.Models.Payment
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public Guid CartId { get; set; }
        public required string Authority { get; set; }
        public int Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public string? RefId { get; set; }
        public string? CardPan { get; set; }

        // Navigation
        public MyApplicationUser User { get; set; } = null!;
        public Cart Cart { get; set; } = null!;
    }
}
