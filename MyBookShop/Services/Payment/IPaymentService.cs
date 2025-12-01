using MyBookShop.Models.Common;
using MyBookShop.Models.Payment;

namespace MyBookShop.Services.Payment
{
    public interface IPaymentService
    {
        Task<ServiceResult<CreatePaymetResponseDto>> CreatePaymentAsync(string cartId, string userId, string callbackUrl);
        Task<ServiceResult<VerifyPaymentResponseDto>> VerifyPaymentAsync(string authority, string status);
    }
}
