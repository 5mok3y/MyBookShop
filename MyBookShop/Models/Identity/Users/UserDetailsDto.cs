namespace MyBookShop.Models.Identity.Users
{
    public class UserDetailsDto
    {
        public required string Id { get; set; }
        public required string FullName { get; set; }
        public required string NationalCode { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required List<string> Roles { get; set; } = new();
    }
}
