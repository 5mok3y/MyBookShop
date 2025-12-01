using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Library.Authors
{
    public class AuthorCreateDto
    {
        [Required]
        [MinLength(1)]
        public required string FullName { get; set; }
    }
}
