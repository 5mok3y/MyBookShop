using MyBookShop.Models.Library.Books;

namespace MyBookShop.Models.Library.Authors
{
    public class AuthorToBook
    {
        public int BookId { get; set; }
        public int AuthorId { get; set; }


        // Navigation properties

        public Author Author { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}
