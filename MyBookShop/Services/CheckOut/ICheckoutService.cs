using MyBookShop.Models.Common;
using MyBookShop.Models.Payment;
using MyBookShop.Models.Shop.CheckOut;

namespace MyBookShop.Services.CheckOut
{
    public interface ICheckoutService
    {
        Task<ServiceResult<CheckoutResultDto>> CheckoutCartAsync(string userId, string cartId, VerifyPaymentResponseDto paymentResponse);
    }
}
