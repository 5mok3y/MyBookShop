using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBookShop.Models.Payment;
using MyBookShop.Services.Payment;
using System.Security.Claims;

namespace MyBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController(IPaymentService _paymentService) : ControllerBase
    {
        [HttpPost("CreatePayment")]
        public async Task<ActionResult<PaymentResponseDto>> CreatePayment(PaymentRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            string callbackUrl = Url.Action("VerifyPayment", "Payment", new { cartId = request.CartId }, Request.Scheme)!;

            var paymentResult = await _paymentService.CreatePaymentAsync(request.CartId, userId!, callbackUrl);

            if (!paymentResult.Success)
            {
                return BadRequest("Failed to initiate payment.");
            }

            var paymentResponse = new PaymentResponseDto()
            {
                PaymentUrl = paymentResult.Response!.PaymentUrl
            };

            return Ok(paymentResponse);
        }

        [AllowAnonymous]
        [HttpGet("VerifyPayment")]
        public async Task<ActionResult<VerifiedPaymentResponseDto>> VerifyPayment(string authority, string status)
        {
            var result = await _paymentService.VerifyPaymentAsync(authority, status);

            if (!result.Success)
            {
                return BadRequest("Paymanet verification failed");
            }

            var verifiedResponse = new VerifiedPaymentResponseDto()
            {
                RefId = result.Response!.RefId
            };

            return Ok(verifiedResponse);
        }
    }
}
