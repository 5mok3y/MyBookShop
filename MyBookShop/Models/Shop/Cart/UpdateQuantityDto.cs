using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Shop.Cart
{
    public class UpdateQuantityDto
    {
        public int BookId { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
