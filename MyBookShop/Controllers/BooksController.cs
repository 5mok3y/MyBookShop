using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBookShop.Data.Context;
using MyBookShop.Models.Library.Authors;
using MyBookShop.Models.Library.Books;
using MyBookShop.Services.Media;

namespace MyBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class BooksController(AppDbContext _context, IImageService _imageService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Book>>> GetAllBooks()
        {
            var books = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.Images)
                .Select(b => new BookResponseDto()
                {
                    Id = b.Id,
                    Title = b.Title,
                    PublishYear = b.PublishYear,
                    Price = b.Price,
                    Quantity = b.Quantity,

                    Authors = b.Authors.Select(a => new AuthorResponseDto()
                    {
                        Id = a.Id,
                        FullName = a.FullName
                    }).ToList(),

                    Images = b.Images.Select(i => new BookImageResponseDto()
                    {
                        Id = i.Id,
                        ImagePath = i.ImagePath,
                        IsCoverImage = i.IsCoverImage
                    }).ToList()
                }).ToListAsync();

            return Ok(books);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BookResponseDto>> GetBookById(int id)
        {
            var book = await _context.Books.Where(b => b.Id == id)
                .Include(b => b.Authors)
                .Include(b => b.Images)
                .Select(b => new BookResponseDto()
            {
                Id = b.Id,
                Title = b.Title,
                PublishYear = b.PublishYear,
                Price = b.Price,
                Quantity = b.Quantity,

                Authors = b.Authors.Select(a => new AuthorResponseDto()
                {
                    Id = a.Id,
                    FullName = a.FullName
                }).ToList(),

                Images = b.Images.Select(i => new BookImageResponseDto()
                {
                    Id = i.Id,
                    ImagePath = i.ImagePath,
                    IsCoverImage = i.IsCoverImage
                }).ToList()
            }).SingleOrDefaultAsync();

            if (book is null)
            {
                return NotFound(new { message = "Book NotFound" });
            }
            return Ok(book);
        }

        [HttpGet("AuthorBooks/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BookResponseDto>> GetBooksByAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author is null)
            {
                return BadRequest("Invalid Author ID");
            }

            var books = await _context.Books.Where(a => a.Authors.Contains(author))
                .Include(b => b.Authors)
                .Include(b => b.Images)
                .Select(b => new BookResponseDto()
            {
                Id = b.Id,
                Title = b.Title,
                PublishYear = b.PublishYear,
                Price = b.Price,
                Quantity = b.Quantity,

                Authors = b.Authors.Select(a => new AuthorResponseDto()
                {
                    Id = a.Id,
                    FullName = a.FullName
                }).ToList(),

                Images = b.Images.Select(i => new BookImageResponseDto()
                {
                    Id = i.Id,
                    ImagePath = i.ImagePath,
                    IsCoverImage = i.IsCoverImage
                }).ToList()
            }).ToListAsync();

            return Ok(books);
        }

        [HttpPost]
        public async Task<ActionResult<BookResponseDto>> AddBook([FromForm] BookCreateDto newBook)
        {
            if (newBook is null)
            {
                return BadRequest("Invalid Inputs");
            }

            if (newBook.Images == null || newBook.Images.Count == 0)
            {
                return BadRequest("You must upload at least one image for the book");
            }
            if (newBook.CoverImageIndex < 0 || newBook.CoverImageIndex >= newBook.Images.Count)
            {
                return BadRequest("Invalid cover image index.");
            }

            var authors = await _context.Authors
                .Where(a => newBook.AuthorIds.Contains(a.Id))
                .ToListAsync();

            if (authors.Count != newBook.AuthorIds.Count)
            {
                return BadRequest("One or more author IDs are invalid.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var book = new Book
                {
                    Title = newBook.Title,
                    Authors = authors,
                    PublishYear = newBook.PublishYear,
                    Price = newBook.Price,
                    Quantity = newBook.Quantity,
                    Images = new()
                };

                _context.Books.Add(book);

                for (int i = 0; i < newBook.Images.Count; i++)
                {
                    var uploadResult = await _imageService.UploadImageAsync(newBook.Images[i]);

                    if (!uploadResult.Success)
                    {
                        return BadRequest($"Image upload failed: {string.Join(", ", uploadResult.Errors ?? new List<string>())}");
                    }
                        
                    book.Images.Add(new BookImage
                    {
                        ImagePath = uploadResult.Response!.ImagePath,
                        IsCoverImage = i == newBook.CoverImageIndex
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = new BookResponseDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    PublishYear = book.PublishYear,
                    Price = book.Price,
                    Quantity = book.Quantity,

                    Authors = authors.Select(a => new AuthorResponseDto
                    {
                        Id = a.Id,
                        FullName = a.FullName
                    }).ToList(),

                    Images = book.Images.Select(i => new BookImageResponseDto
                    {
                        Id = i.Id,
                        ImagePath = i.ImagePath,
                        IsCoverImage = i.IsCoverImage
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, result);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> EditBook(int id, BookEditDto editedBook)
        {
            if (editedBook is null)
            {
                return BadRequest("Invalid Inputs");
            }

            var book = await _context.Books.FindAsync(id);

            if (book is null)
            {
                return BadRequest("Book NotFound");
            }

            if (editedBook.Title is not null)
            {
                book.Title = editedBook.Title;
            }

            if (editedBook.PublishYear is not null)
            {
                book.PublishYear = editedBook.PublishYear;
            }

            if (editedBook.Price is not null && editedBook.Price > 0)
            {
                book.Price = editedBook.Price.Value;
            }

            if (editedBook.Quantity is not null && editedBook.Price >= 0)
            {
                book.Quantity = editedBook.Quantity.Value;
            }

            if (editedBook.AuthorIds is not null)
            {
                var authors = await _context.Authors
                    .Where(a => editedBook.AuthorIds!.Contains(a.Id))
                    .ToListAsync();

                book.Authors = authors;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book is null)
            {
                return BadRequest("Book not found");
            }

            var failedDeletes = new List<string>();

            foreach (var image in book.Images)
            {
                var result = _imageService.DeleteImage(image.ImagePath);
                if (!result.Success)
                {
                    failedDeletes.Add($"Failed to delete book image: {string.Join(", ", result.Errors!)}");
                }

                _context.BookImages.Remove(image);
            }

            _context.Remove(book);
            await _context.SaveChangesAsync();

            if (failedDeletes.Any())
            {
                return BadRequest(new { Errors = failedDeletes });
            }

            return NoContent();
        }

        [HttpPost("{bookId}/Images")]
        public async Task<ActionResult<List<BookImageResponseDto>>> AddBookImage(int bookId, [FromForm] BookImageAddDto dto)
        {
            var book = await _context.Books
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book is null)
            {
                return BadRequest("Book not found");
            }

            var uploadedImages = new List<BookImage>();

            for (int i = 0; i < dto.Images.Count; i++)
            {
                var image = dto.Images[i];

                var uploadResult = await _imageService.UploadImageAsync(image);
                if (!uploadResult.Success)
                {
                    return BadRequest($"Image upload failed: {string.Join(", ", uploadResult.Errors ?? new List<string>())}");
                }

                uploadedImages.Add(new BookImage
                {
                    ImagePath = uploadResult.Response!.ImagePath,
                    IsCoverImage = dto.CoverImageIndex.HasValue ? (i == dto.CoverImageIndex) : false
                });
            }

            book.Images.AddRange(uploadedImages);
            await _context.SaveChangesAsync();

            var response = uploadedImages.Select(i => new BookImageResponseDto
            {
                Id = i.Id,
                ImagePath = i.ImagePath,
                IsCoverImage = i.IsCoverImage
            }).ToList();

            return Ok(response);
        }

        [HttpDelete("{bookId}/Images/")]
        public async Task<ActionResult> DeleteBookImages(int bookId, BookImageDeleteDto dto)
        {
            var book = await _context.Books
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book is null)
            {
                return BadRequest("Book not found");
            }

            var notFoundIds = new List<Guid>();
            var coverSkippedIds = new List<Guid>();

            foreach (var imageId in dto.ImageIds)
            {
                var image = book.Images.FirstOrDefault(i => i.Id == imageId);
                if (image is null)
                {
                    notFoundIds.Add(imageId);
                    continue;
                }

                if (image.IsCoverImage)
                {
                    coverSkippedIds.Add(imageId);
                    continue;
                }

                var result = _imageService.DeleteImage(image.ImagePath);
                if (!result.Success)
                {
                    return BadRequest($"Failed to delete ImageId {image.Id}: {string.Join(", ", result.Errors!)}");
                }

                _context.BookImages.Remove(image);
            }

            await _context.SaveChangesAsync();

            if (notFoundIds.Any() || coverSkippedIds.Any())
            {
                return BadRequest(new
                {
                    NotFoundIds = notFoundIds,
                    CoverSkippedIds = coverSkippedIds,
                    Message = "Some images were not deleted"
                });
            }

            return NoContent();
        }

        [HttpPut("{bookId}/CoverImage/")]
        public async Task<ActionResult> SetCoverImage(int bookId, BookImageSetCoverDto dto)
        {
            var book = await _context.Books
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book is null)
            {
                return BadRequest("Book not found");
            }

            var newCoverImage = book.Images.FirstOrDefault(i => i.Id == dto.ImageId);

            if (newCoverImage is null)
            {
                return BadRequest("Image not found in the specified book");
            }

            foreach (var image in book.Images)
            {
                image.IsCoverImage = (image.Id == dto.ImageId);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
