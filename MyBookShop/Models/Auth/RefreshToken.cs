using MyBookShop.Models.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBookShop.Models.Auth
{
    public class RefreshToken
    {
        public Guid id { get; set; }
        public required string TokenHash { get; set; }
        public bool IsRevoked { get; set; }
        public required string UserId { get; set; }
        public string AccessTokenId { get; set; } = string.Empty;
        public required DateTime CreatedAt { get; set; }
        public required DateTime ExpiresAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public MyApplicationUser? User { get; set; }

    }
}
