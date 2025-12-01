using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBookShop.Data.Context;
using MyBookShop.Models.Shop.Order;
using System.Security.Claims;

namespace MyBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController(AppDbContext _context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<OrderListDto>> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders.Where(o => o.UserId == userId).Select(o => new OrderListDto()
            {
                Id = o.Id.ToString(),
                RefId = o.RefId,
                CreatedAt = o.CreatedAt,
                TotalPrice = o.TotalPrice,
                Status = o.Status
            }
            ).ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<string>> GetOrderById(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Book)
                .Where(o => o.UserId == userId)
                .Where(o => o.Id == id)
                .Select(o => new OrderDetailsDto()
                {
                    Id = o.Id.ToString(),
                    CreatedAt = o.CreatedAt,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    RefId = o.RefId,

                    OrderItems = o.OrderItems.Select(i => new OrderItemsDto()
                    {
                        BookId = i.BookId,
                        BookTitle = i.Book.Title,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity
                    }
                ).ToList()
                }).FirstOrDefaultAsync();

            if (order is null)
            {
                return BadRequest("Order not found");
            }

            return Ok(order);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditOrderStatus(Guid id, OrderStatusDto status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order is null)
            {
                return BadRequest("Order not found");
            }

            order.Status = status.OrderStatus;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
