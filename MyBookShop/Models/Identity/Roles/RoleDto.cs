using System.ComponentModel.DataAnnotations;

namespace MyBookShop.Models.Identity.Roles
{
    public class RoleDto
    {
        [Required]
        [MinLength(1)]
        public required string RoleName { get; set; }
    }
}
