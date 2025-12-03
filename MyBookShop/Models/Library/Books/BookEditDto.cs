using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Library.Books
{
    public class BookEditDto
    {
        public string? Title { get; set; }

        public List<int>? AuthorIds { get; set; }

        public string? PublishYear { get; set; }

        public int? Price { get; set; }

        public int? Quantity { get; set; }

    }
}
