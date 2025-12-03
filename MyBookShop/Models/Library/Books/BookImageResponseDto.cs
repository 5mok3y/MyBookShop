namespace MyBookShop.Models.Library.Books
{
    public class BookImageResponseDto
    {
        public required Guid Id { get; set; }
        public required string ImagePath { get; set; }
        public required bool IsCoverImage { get; set; }
    }
}
