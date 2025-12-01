using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBookShop.Models.Identity.Roles;

namespace MyBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController(RoleManager<IdentityRole> _roleManager) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.Select(r => new RoleDto()
            {
                RoleName = r.Name!
            }).ToListAsync();

            return Ok(roles);
        }

        [HttpGet("{roleName}", Name = "GetRole")]
        public async Task<ActionResult<RoleDto>> GetRoleByNameAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                return NotFound("Role Not Found");
            }
            var response = new RoleDto()
            {
                RoleName = role.Name!
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<RoleDto>> AddRoleAsync(RoleDto request)
        {
            var role = new IdentityRole()
            {
                Name = request.RoleName
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return CreatedAtRoute("GetRole", new { roleName = role.Name }, new RoleDto() { RoleName = role.Name });
        }

        [HttpPut("{roleName}")]
        public async Task<ActionResult> EditRoleAsync(string roleName, RoleDto request)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                return NotFound("Role Not Found");
            }

            role.Name = request.RoleName;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpDelete("{roleName}")]
        public async Task<ActionResult> DeleteRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                return NotFound("Role Not Found");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
