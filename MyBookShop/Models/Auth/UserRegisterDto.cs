using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Auth
{
    public class UserRegisterDto
    {
        [Required]
        [EmailAddress]
        [MinLength(1)]
        public required string Email { get; set; }

        [Required]
        [MinLength(1)]
        public required string UserName { get; set; }

        [Required]
        [MinLength(1)]
        [DataType(DataType.PhoneNumber)]
        public required string PhoneNumber { get; set; }

        [Required]
        [MinLength(1)]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required]
        [MinLength(1)]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public required string ConfirmPassword { get; set; }

        [Required]
        [MinLength(1)]
        public required string FullName { get; set; }

        [Required]
        [MinLength(1)]
        public required string NationalCode { get; set; }
    }
}
