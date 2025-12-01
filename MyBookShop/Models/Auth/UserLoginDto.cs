using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Auth
{
    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        [MinLength(1)]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(1)]
        public required string Password { get; set; }
    }
}
