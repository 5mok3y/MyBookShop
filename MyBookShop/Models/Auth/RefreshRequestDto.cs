using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Auth
{
    public class RefreshRequestDto
    {
        [Required]
        [MinLength(1)]
        public required string AccessToken { get; set; }

        [Required]
        [MinLength(1)]
        public required string RefreshToken { get; set; }
    }
}
