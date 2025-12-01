namespace MyBookShop.Models.Payment
{
    public class VerifyPaymentResponseDto
    {
        public required string RefId { get; set; }
        public string? CardPan { get; set; }
    }
}
