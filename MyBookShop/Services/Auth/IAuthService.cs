using MyBookShop.Models.Auth;
using MyBookShop.Models.Common;

namespace MyBookShop.Services.Auth
{
    public interface IAuthService
    {
        Task<ServiceResult<RegisterResponseDto>> RegisterAsync(UserRegisterDto register);
        Task<ServiceResult<LoginResponseDto>> LoginAsync(UserLoginDto login);
        Task<ServiceResult<LoginResponseDto>> RefreshTokensAsync(RefreshRequestDto request);
        Task<ServiceResult<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request);
    }
}
