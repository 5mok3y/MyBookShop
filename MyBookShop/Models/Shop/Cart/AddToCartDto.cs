using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Shop.Cart
{
    public class AddToCartDto
    {
        public int BookId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
