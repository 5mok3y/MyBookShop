using MyBookShop.Models.Library.Authors;
using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Library.Books
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Year must be a 4-digit number")]
        public required string PublishYear { get; set; }

        [Required]
        public required int Price { get; set; }

        [Required]
        public required int Quantity { get; set; }

        // Navigation
        public List<Author> Authors { get; set; } = new();
        public List<BookImage> Images { get; set; } = new();
    }
}
