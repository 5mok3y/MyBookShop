using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Payment
{
    public class PaymentRequestDto
    {
        [Required]
        [MinLength(1)]
        public required string CartId { get; set; }
    }
}
