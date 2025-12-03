namespace MyBookShop.Models.Library.Books
{
    public class BookImage
    {
        public Guid Id { get; set; }
        public int BookId { get; set; }
        public required string ImagePath { get; set; }
        public bool IsCoverImage { get; set; }

        // Navigation
        public Book Book { get; set; } = null!;
    }
}
