using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Library.Books
{
    public class BookImageSetCoverDto
    {
        [Required]
        public required Guid ImageId { get; set; }
    }
}
