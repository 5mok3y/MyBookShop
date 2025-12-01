namespace MyBookShop.Models.Payment
{
    public class CreatePaymetResponseDto
    {
        public required string PaymentUrl { get; set; }
        public string? Authority { get; set; }
    }
}
