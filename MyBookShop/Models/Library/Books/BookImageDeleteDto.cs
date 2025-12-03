using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Library.Books
{
    public class BookImageDeleteDto
    {
        [Required]
        public required List<Guid> ImageIds { get; set; }
    }
}
