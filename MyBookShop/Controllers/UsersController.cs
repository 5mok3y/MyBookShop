using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBookShop.Models.Identity;
using MyBookShop.Models.Identity.Roles;
using MyBookShop.Models.Identity.Users;

namespace MyBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController(UserManager<MyApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<UserListDto>>> GetAllUsers()
        {
            var users = await _userManager.Users.Select(u => new UserListDto()
            {
                FullName = u.FullName,
                NationalCode = u.NationalCode,
                UserName = u.UserName!,
                Email = u.Email!,
                PhoneNumber = u.PhoneNumber!,
            }).ToListAsync();

            return Ok(users);
        }

        [HttpGet("{userName}")]
        public async Task<ActionResult<UserDetailsDto>> GetUser(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDetailsDto()
            {
                Id = user.Id,
                FullName = user.FullName,
                NationalCode = user.NationalCode,
                UserName = user.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                Roles = roles.ToList()
            };

            return Ok(userDto);
        }

        [HttpPost("{userName}/AddRole")]
        public async Task<ActionResult> AddUserToRole(string userName, RoleDto request)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                return BadRequest("User not found");
            }

            var role = await _roleManager.FindByNameAsync(request.RoleName);
            if (role is null)
            {
                return BadRequest("Role not found");
            }

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpPost("{userName}/RemoveRole")]
        public async Task<ActionResult> RemoveUserGromRole(string userName, RoleDto request)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                return BadRequest("User not found");
            }

            var role = await _roleManager.FindByNameAsync(request.RoleName);
            if (role is null)
            {
                return BadRequest("Role not found");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
