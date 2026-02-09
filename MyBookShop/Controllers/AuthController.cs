using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBookShop.Models.Auth;
using MyBookShop.Services.Auth;
using System.Security.Claims;

namespace MyBookShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService _authService) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<ActionResult<RegisterResponseDto>> RegisterAsync(UserRegisterDto register)
        {
            var result = await _authService.RegisterAsync(register);
            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Response);
        }


        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponseDto>> LoginAsync(UserLoginDto login)
        {
            var result = await _authService.LoginAsync(login);
            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Response);
        }

        [HttpPost("RefreshTokens")]
        public async Task<ActionResult<LoginResponseDto>> RefreshTokensAsync(RefreshRequestDto request)
        {
            var result = await _authService.RefreshTokensAsync(request);
            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Response);
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<ActionResult> ChangePasswordAsync(ChangePasswordDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _authService.ChangePasswordAsync(userId!, request);
            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result);
        }
    }
}
