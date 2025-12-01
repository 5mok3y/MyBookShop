using Microsoft.EntityFrameworkCore;
using MyBookShop.Data.Context;
using MyBookShop.Models.Common;
using MyBookShop.Models.Enums;
using MyBookShop.Models.Payment;
using MyBookShop.Services.CheckOut;
using MyBookShop.Services.Gateway.Zarinpal;

namespace MyBookShop.Services.Payment
{
    public class PaymentService(Zarinpal _zarinpal, AppDbContext _context, ICheckoutService _checkoutService) : IPaymentService
    {
        public async Task<ServiceResult<CreatePaymetResponseDto>> CreatePaymentAsync(string cartId, string userId, string callbackUrl)
        {
            if (!Guid.TryParse(cartId, out var cartIdGuid))
            {
                return ServiceResult<CreatePaymetResponseDto>.Failed(new() { "Invalid cartId" });
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartIdGuid);

            if (cart is null)
            {
                return ServiceResult<CreatePaymetResponseDto>.Failed(new() { "Cart not found" });
            }

            if (cart.UserId != userId)
            {
                return ServiceResult<CreatePaymetResponseDto>.Failed(new() { "Access denied" });
            }

            if (cart.CartItems.Count == 0)
            {
                return ServiceResult<CreatePaymetResponseDto>.Failed(new() { "Cart is empty" });
            }

            var user = await _context.Users.FindAsync(userId);

            var amount = cart.CartItems.Sum(i => i.GetTotalPrice());

            var result = await _zarinpal.RequestAsync(amount, callbackUrl, $"Payment for cart {cart.Id}", user!.Email, user.PhoneNumber ?? null);

            if (result?.data?.code == 100)
            {
                var transaction = new Transaction()
                {
                    User = user,
                    Cart = cart,
                    Amount = amount,
                    Authority = result.data.authority,
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                var response = new CreatePaymetResponseDto
                {
                    PaymentUrl = _zarinpal.GetPaymentUrl(result.data.authority),
                    Authority = result.data.authority
                };

                return ServiceResult<CreatePaymetResponseDto>.Ok(response);
            }

            return ServiceResult<CreatePaymetResponseDto>.Failed(new() { "Failed to create payment request" });
        }

        public async Task<ServiceResult<VerifyPaymentResponseDto>> VerifyPaymentAsync(string authority, string status)
        {
            if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(authority))
            {
                return ServiceResult<VerifyPaymentResponseDto>.Failed(new() { "Invalid parameters" });
            }

            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Authority == authority);

            if (transaction is null)
            {
                return ServiceResult<VerifyPaymentResponseDto>.Failed(new() { "Transaction not found" });
            }

            if (!status.Equals("ok", StringComparison.InvariantCultureIgnoreCase))
            {
                transaction.Status = PaymentStatus.Canceled;
                await _context.SaveChangesAsync();

                return ServiceResult<VerifyPaymentResponseDto>.Failed(new() { "Payment was not successful" });
            }

            var cartId = transaction.CartId;

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart is null)
            {
                transaction.Status = PaymentStatus.Canceled;
                await _context.SaveChangesAsync();

                return ServiceResult<VerifyPaymentResponseDto>.Failed(new() { "Cart not found" });
            }

            int amount = cart.CartItems.Sum(i => i.GetTotalPrice());

            var verifyResult = await _zarinpal.VerifyAsync(authority, amount);

            if (verifyResult?.data?.code == 100)
            {
                transaction.Status = PaymentStatus.Completed;
                transaction.RefId = verifyResult.data.ref_id.ToString();
                transaction.CardPan = verifyResult.data.card_pan.ToString();
                transaction.PaidAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = new VerifyPaymentResponseDto
                {
                    RefId = verifyResult.data.ref_id.ToString(),
                    CardPan = verifyResult.data.card_pan
                };

                var checkoutResult = await _checkoutService.CheckoutCartAsync(transaction.UserId, cartId.ToString(), response);
                if (!checkoutResult.Success)
                {
                    return ServiceResult<VerifyPaymentResponseDto>.Failed(new List<string>
                    {
                        "Payment verified but order creation failed"
                    }
                    .Concat(checkoutResult.Errors!).ToList());
                }

                return ServiceResult<VerifyPaymentResponseDto>.Ok(response);
            }

            transaction.Status = PaymentStatus.Canceled;

            await _context.SaveChangesAsync();

            return ServiceResult<VerifyPaymentResponseDto>.Failed(new() { "Payment verification failed" });
        }
    }
}
