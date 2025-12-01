using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBookShop.Data.Context;
using MyBookShop.Models.Library.Authors;
using MyBookShop.Models.Library.Books;

namespace MyBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class BooksController(AppDbContext _context) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Book>>> GetAllBooks()
        {
            var books = await _context.Books.Select(b => new BookResponseDto()
            {
                Id = b.Id,
                Title = b.Title,

                Authors = b.Authors.Select(a => new AuthorResponseDto()
                {
                    Id = a.Id,
                    FullName = a.FullName
                }).ToList(),

                PublishYear = b.PublishYear,
                Price = b.Price,
                Quantity = b.Quantity
            }).ToListAsync();

            return Ok(books);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BookResponseDto>> GetBook(int id)
        {
            var book = await _context.Books.Where(b => b.Id == id).Select(b => new BookResponseDto()
            {
                Id = b.Id,
                Title = b.Title,

                Authors = b.Authors.Select(a => new AuthorResponseDto()
                {
                    Id = a.Id,
                    FullName = a.FullName
                }).ToList(),

                PublishYear = b.PublishYear,
                Price = b.Price,
                Quantity = b.Quantity
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

            var books = await _context.Books.Where(a => a.Authors.Contains(author)).Select(b => new BookResponseDto()
            {
                Id = b.Id,
                Title = b.Title,

                Authors = b.Authors.Select(a => new AuthorResponseDto()
                {
                    Id = a.Id,
                    FullName = a.FullName
                }).ToList(),

                PublishYear = b.PublishYear,
                Price = b.Price,
                Quantity = b.Quantity
            }).ToListAsync();

            return Ok(books);
        }

        [HttpPost]
        public async Task<ActionResult<BookResponseDto>> AddBook(BookCreateDto newBook)
        {
            if (newBook is null)
            {
                return BadRequest("Invalid Inputs");
            }

            var authors = await _context.Authors
                .Where(a => newBook.AuthorIds.Contains(a.Id))
                .ToListAsync();

            if (authors.Count != newBook.AuthorIds.Count)
            {
                return BadRequest("One or more author IDs are invalid.");
            }


            var book = new Book()
            {
                Title = newBook.Title,
                Authors = authors,
                PublishYear = newBook.PublishYear,
                Price = newBook.Price,
                Quantity = newBook.Quantity
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var result = new BookResponseDto
            {
                Id = book.Id,
                Title = book.Title,

                Authors = book.Authors.Select(a => new AuthorResponseDto()
                {
                    Id = a.Id,
                    FullName = a.FullName
                }).ToList(),

                PublishYear = book.PublishYear,
                Price = book.Price,
                Quantity = book.Quantity
            };

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> EditBook(int id, BookCreateDto editedBook)
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

            book.Title = editedBook.Title;
            book.Authors = new List<Author>();
            book.PublishYear = editedBook.PublishYear;
            book.Price = editedBook.Price;
            book.Quantity = editedBook.Quantity;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book is null)
            {
                return BadRequest("Book not found");
            }
            _context.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
