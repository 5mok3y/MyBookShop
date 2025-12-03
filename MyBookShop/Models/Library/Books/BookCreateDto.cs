using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Library.Books
{
    public class BookCreateDto
    {
        [Required]
        [MinLength(1)]
        public required string Title { get; set; }

        public List<int> AuthorIds { get; set; } = new();

        [Required]
        [MinLength(1)]
        public required string PublishYear { get; set; }

        [Range(1, int.MaxValue)]
        public required int Price { get; set; }

        [Required]
        public required int Quantity { get; set; }

        public List<IFormFile> Images { get; set; } = new();

        public int CoverImageIndex { get; set; }
    }
}
