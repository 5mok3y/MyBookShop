namespace MyBookShop.Models.Auth
{
    public class RegisterResponseDto
    {
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string FullName { get; set; }
        public required string NationalCode { get; set; }
    }
}
