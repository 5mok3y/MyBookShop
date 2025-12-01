namespace MyBookShop.Models.Auth
{
    public class RefreshTokenDto
    {
        public required string RefreshToken { get; set; }
        public required string RefreshTokenHash { get; set; }
    }
}
