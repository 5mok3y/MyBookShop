using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBookShop.Data.Context;
using MyBookShop.Models.Library.Authors;

namespace MyBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuthorsController(AppDbContext _context) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<AuthorResponseDto>>> GetAllAuthors()
        {
            var authors = await _context.Authors.Select(a => new AuthorResponseDto()
            {
                Id = a.Id,
                FullName = a.FullName
            }).ToListAsync();

            return Ok(authors);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthorResponseDto>> GetAuthor(int id)
        {
            var author = await _context.Authors.Where(a => a.Id == id).Select(a => new AuthorResponseDto()
            {
                Id = a.Id,
                FullName = a.FullName
            }).SingleOrDefaultAsync();

            if (author is null)
            {
                return NotFound(new { message = "Author NotFound" });
            }

            return Ok(author);
        }

        [HttpPost]
        public async Task<ActionResult<AuthorResponseDto>> AddAuthor(AuthorCreateDto newAuthor)
        {
            if (newAuthor is null)
            {
                return BadRequest("Invalid Inputs");
            }

            var author = new Author()
            {
                FullName = newAuthor.FullName
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var result = new AuthorResponseDto()
            {
                Id = author.Id,
                FullName = author.FullName
            };

            return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> EditAuthor(int id, AuthorCreateDto editedAuthor)
        {
            if (editedAuthor is null)
            {
                return BadRequest("Invalid Inputs");
            }

            var author = await _context.Authors.FindAsync(id);
            if (author is null)
            {
                return BadRequest("Author NotFound");
            }

            author.FullName = editedAuthor.FullName;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author is null)
            {
                return BadRequest("Author not found");
            }
            _context.Remove(author);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
