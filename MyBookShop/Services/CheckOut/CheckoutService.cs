using Microsoft.EntityFrameworkCore;
using MyBookShop.Data.Context;
using MyBookShop.Models.Common;
using MyBookShop.Models.Payment;
using MyBookShop.Models.Shop.CheckOut;
using MyBookShop.Models.Shop.Order;

namespace MyBookShop.Services.CheckOut
{
    public class CheckoutService(AppDbContext _context) : ICheckoutService
    {
        public async Task<ServiceResult<CheckoutResultDto>> CheckoutCartAsync(string userId, string cartId, VerifyPaymentResponseDto paymentResponse)
        {
            if (!Guid.TryParse(cartId, out var cartIdGuid))
            {
                return ServiceResult<CheckoutResultDto>.Failed(new() { "Invalid cartId" });
            }

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.Id == cartIdGuid);

            if (userCart is null)
            {
                return ServiceResult<CheckoutResultDto>.Failed(new() { "Cart not found" });
            }

            if (userCart.UserId != userId)
            {
                return ServiceResult<CheckoutResultDto>.Failed(new() { "Access denied" });
            }

            userCart.IsCheckedOut = true;
            userCart.CheckedOutDate = DateTime.UtcNow;

            var order = new Order()
            {
                UserId = userCart.UserId,
                CreatedAt = DateTime.UtcNow,
                CardPan = paymentResponse.CardPan,
                RefId = paymentResponse.RefId,

                OrderItems = userCart.CartItems.Select(i => new OrderItem()
                {
                    BookId = i.BookId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                }).ToList(),

                TotalPrice = userCart.CartItems.Sum(i => i.GetTotalPrice()),
            };

            _context.Orders.Add(order);

            foreach (var item in userCart.CartItems)
            {
                item.Book.Quantity -= item.Quantity;
            }

            await _context.SaveChangesAsync();

            return ServiceResult<CheckoutResultDto>.Ok(new CheckoutResultDto
            {
                OrderId = order.Id.ToString(),
                CreatedAt = order.CreatedAt,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                RefId = order.RefId
            });
        }
    }
}
