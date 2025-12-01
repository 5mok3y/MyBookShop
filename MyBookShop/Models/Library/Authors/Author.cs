using MyBookShop.Models.Library.Books;
using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Library.Authors
{
    public class Author
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FullName { get; set; }

        public List<Book> Books { get; set; } = new();
    }
}
