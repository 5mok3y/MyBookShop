using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBookShop.Data.Context;
using MyBookShop.Models.Shop.Cart;
using System.Security.Claims;

namespace MyBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController(AppDbContext _context) : ControllerBase
    {
        //AddBookToCart
        [HttpPost("Add")]
        public async Task<ActionResult> AddItemToCart(AddToCartDto request)
        {
            if (request is null)
            {
                return BadRequest("Invalid Inputs");
            }

            var book = await _context.Books.FindAsync(request.BookId);
            if (book is null)
            {
                return BadRequest("Invalid Book ID");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (userCart is null)
            {
                userCart = new Cart()
                {
                    UserId = userId!,
                    CartItems = new()
                };

                _context.Carts.Add(userCart);
            }

            var cartItem = userCart.CartItems
                .FirstOrDefault(i => i.BookId == request.BookId);

            if (cartItem is null)
            {
                cartItem = new CartItem()
                {
                    Cart = userCart,
                    BookId = request.BookId,
                    Quantity = request.Quantity,
                    UnitPrice = book.Price
                };

                userCart.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += request.Quantity;
            }

            if (request.Quantity > book.Quantity)
            {
                return BadRequest("Cart quantity cannot be more than stock quantity.");
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }


        //RemoveItem
        [HttpDelete("Remove/{bookId}")]
        public async Task<ActionResult> RemoveItemFromCart(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (userCart is null)
            {
                return NotFound("Cart not found.");
            }

            var cartItem = userCart.CartItems
                .FirstOrDefault(i => i.BookId == bookId);

            if (cartItem is null)
            {
                return NotFound("Cart item not found.");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //UpdateQuantity
        [HttpPut("Update-Quantity")]
        public async Task<ActionResult> UpdateItemQuantity(UpdateQuantityDto request)
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.UserId == user && !c.IsCheckedOut);

            if (userCart is null)
            {
                return NotFound("Cart not found.");
            }

            var cartItem = userCart.CartItems.FirstOrDefault(i => i.BookId == request.BookId);

            if (cartItem is null)
            {
                return NotFound("Cart item not found.");
            }

            var bookQuantityInstock = cartItem.Book.Quantity;

            if (request.Quantity > bookQuantityInstock)
            {
                return BadRequest("Cart quantity cannot be more than stock quantity.");
            }

            if (request.Quantity > 0)
            {
                cartItem.Quantity = request.Quantity;
            }
            else
            {
                _context.CartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }


        //GetCart
        [HttpGet("User-Cart")]
        public async Task<ActionResult<CartResponseDto>> GetUserActiveCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (userCart is null)
            {
                return NotFound("Cart not found.");
            }

            var result = new CartResponseDto
            {
                Id = userCart.Id.ToString(),
                CreatedAt = userCart.CreatedAt,
                IsCheckedOut = userCart.IsCheckedOut,

                CartItems = userCart.CartItems.Select(i => new CartItemResponseDto()
                {
                    BookId = i.BookId,
                    BookTitle = i.Book.Title,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList(),

                TotalPrice = userCart.CartItems.Sum(i => i.GetTotalPrice())
            };

            return Ok(result);
        }

        [HttpDelete("Clear-Cart")]
        //ClearCart
        public async Task<ActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (userCart is null)
            {
                return NotFound("Cart not found.");
            }

            _context.CartItems.RemoveRange(userCart.CartItems);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("CheckOut-Cart")]
        public async Task<ActionResult<CartCheckoutDto>> CheckOutCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userCart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (userCart is null)
            {
                return NotFound("Cart not found.");
            }

            var response = new CartCheckoutDto
            {
                CartId = userCart.Id.ToString()
            };

            return Ok(response);
        }
    }
}
