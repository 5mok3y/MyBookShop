using MyBookShop.Models.Library.Authors;

namespace MyBookShop.Models.Library.Books
{
    public class BookResponseDto
    {

        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string PublishYear { get; set; }
        public required int Price { get; set; }
        public required int Quantity { get; set; }
        public required List<AuthorResponseDto> Authors { get; set; } = new();
        public required List<BookImageResponseDto> Images { get; set; } = new();
    }
}
