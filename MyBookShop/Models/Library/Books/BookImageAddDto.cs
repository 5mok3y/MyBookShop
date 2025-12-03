using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Library.Books
{
    public class BookImageAddDto
    {
        [Required]
        public required List<IFormFile> Images { get; set; }
        public int? CoverImageIndex { get; set; }
    }
}
