namespace MyBookShop.Models.Shop.Cart
{
    public class CartResponseDto
    {
        public required string Id { get; set; }
        public required DateTime CreatedAt { get; set; }
        public bool IsCheckedOut { get; set; }
        public int TotalPrice { get; set; }
        public required List<CartItemResponseDto> CartItems { get; set; } = new();
        public int TotalQuantity => CartItems.Sum(i => i.Quantity);
    }
}
