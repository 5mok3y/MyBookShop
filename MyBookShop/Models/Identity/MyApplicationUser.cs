using Microsoft.AspNetCore.Identity;
using MyBookShop.Models.Payment;
using MyBookShop.Models.Shop.Cart;
using MyBookShop.Models.Shop.Order;
using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Identity
{
    public class MyApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public required string FullName { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National code must be 10 digits.")]
        public required string NationalCode { get; set; }

        // Navigation
        public List<Cart> Carts { get; set; } = new();
        public List<Transaction> Payments { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
    }
}
