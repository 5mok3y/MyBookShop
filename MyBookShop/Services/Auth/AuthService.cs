using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyBookShop.Data.Context;
using MyBookShop.Models.Auth;
using MyBookShop.Models.Common;
using MyBookShop.Models.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyBookShop.Services.Auth
{
    public class AuthService(

        UserManager<MyApplicationUser> _userManager,
        AppDbContext _context,
        IConfiguration _configuration

        ) : IAuthService
    {
        public async Task<ServiceResult<RegisterResponseDto>> RegisterAsync(UserRegisterDto register)
        {
            if (register is null)
            {
                return ServiceResult<RegisterResponseDto>.Failed(new() { "Input data is null" });
            }

            if (await _userManager.Users.AnyAsync(u => u.NationalCode == register.NationalCode))
            {
                return ServiceResult<RegisterResponseDto>.Failed(new() { "NationalCode already exists" });
            }

            var user = new MyApplicationUser()
            {
                Email = register.Email,
                UserName = register.UserName,
                PhoneNumber = register.PhoneNumber,
                FullName = register.FullName,
                NationalCode = register.NationalCode,
            };

            var result = await _userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();

                return ServiceResult<RegisterResponseDto>.Failed(errors);
            }

            var response = new RegisterResponseDto()
            {
                Email = user.Email,
                UserName = user.UserName,
                FullName = user.FullName,
                NationalCode = user.NationalCode
            };

            return ServiceResult<RegisterResponseDto>.Ok(response);
        }

        public async Task<ServiceResult<LoginResponseDto>> LoginAsync(UserLoginDto login)
        {
            if (login is null)
            {
                return ServiceResult<LoginResponseDto>.Failed(new() { "Input data is null" });
            }

            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, login.Password))
            {
                return ServiceResult<LoginResponseDto>.Failed(new() { "Wrong Username or Password" });
            }

            var response = await GenerateTokensAsync(user);
            if (response is null)
            {
                return ServiceResult<LoginResponseDto>.Failed(new() { "Something went wrong, Please try again" });
            }

            return ServiceResult<LoginResponseDto>.Ok(response);
        }

        public async Task<ServiceResult<LoginResponseDto>> RefreshTokensAsync(RefreshRequestDto request)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();

                if (!jwtHandler.CanReadToken(request.AccessToken))
                {
                    return ServiceResult<LoginResponseDto>.Failed(new() { "Invalid AccessToken" });
                }

                jwtHandler.ValidateToken(request.AccessToken,
                    new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!)),
                        ValidIssuer = _configuration["Jwt:Issuer"],
                        ValidAudience = _configuration["Jwt:Audience"],

                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,

                        ValidateLifetime = false

                    }, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken)
                {
                    return ServiceResult<LoginResponseDto>.Failed(new() { "Invalid AccessToken" });
                }

                var jti = jwtToken.Id;
                var userId = jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    return ServiceResult<LoginResponseDto>.Failed(new() { "User not found" });
                }

                var userRefreshTokens = await _context.RefreshTokens
                    .Where(r => r.UserId == userId)
                    .Where(r => !r.IsRevoked)
                    .Where(r => r.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

                var storedRefreshToken = userRefreshTokens.FirstOrDefault(t => VerifyTokenHash(request.RefreshToken, t.TokenHash));
                if (storedRefreshToken is null)
                {
                    return ServiceResult<LoginResponseDto>.Failed(new() { "Invalid RefreshToken" });
                }

                if (storedRefreshToken.AccessTokenId != jti)
                {
                    return ServiceResult<LoginResponseDto>.Failed(new() { "Tokens do not match" });
                }

                var response = await GenerateTokensAsync(user);
                if (response is null)
                {
                    return ServiceResult<LoginResponseDto>.Failed(new() { "Something went wrong, Please try again" });
                }

                storedRefreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();

                return ServiceResult<LoginResponseDto>.Ok(response);
            }

            catch (SecurityTokenException)
            {
                return ServiceResult<LoginResponseDto>.Failed(new() { "Invalid AccessToken" });
            }

            catch (ArgumentException)
            {
                return ServiceResult<LoginResponseDto>.Failed(new() { "Invalid AccessToken" });
            }

            catch (Exception)
            {
                return ServiceResult<LoginResponseDto>.Failed(new() { "Internal server error, please try again later" });
            }
        }

        private async Task<LoginResponseDto> GenerateTokensAsync(MyApplicationUser user)
        {
            //Generate AccessToken

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                signingCredentials: creds,
                expires: DateTime.UtcNow.AddMinutes(10)
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);


            // Generate RefreshToken

            var randomToken = GenerateRefreshTokenAndHash();

            var refreshToken = new RefreshToken()
            {
                TokenHash = randomToken.RefreshTokenHash,
                AccessTokenId = tokenDescriptor.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new LoginResponseDto()
            {
                AccessToken = accessToken,
                RefreshToken = randomToken.RefreshToken,
                ExpiresAt = tokenDescriptor.ValidTo
            };
        }

        private static RefreshTokenDto GenerateRefreshTokenAndHash()
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var hashBytes = SHA512.HashData(Encoding.UTF8.GetBytes(refreshToken));

            var refreshTokenHash = Convert.ToBase64String(hashBytes);

            return new RefreshTokenDto()
            {
                RefreshToken = refreshToken,
                RefreshTokenHash = refreshTokenHash
            };
        }

        private bool VerifyTokenHash(string providedToken, string storedTokenHash)
        {
            var providedHashBytes = SHA512.HashData(Encoding.UTF8.GetBytes(providedToken));

            var storedHashBytes = Convert.FromBase64String(storedTokenHash);

            return CryptographicOperations.FixedTimeEquals(providedHashBytes, storedHashBytes);
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request)
        {
            if (request is null)
            {
                return ServiceResult<bool>.Failed(new() { "Ivalid parameters" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return ServiceResult<bool>.Failed(new() { "User not found" });
            }

            if (request.NewPassword == request.CurrentPassword)
            {
                return ServiceResult<bool>.Failed(
                    new() { "New password must be different from current password" });
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ServiceResult<bool>.Failed(errors);
            }

            await _context.RefreshTokens    
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ExecuteUpdateAsync(t =>
            t.SetProperty(x => x.IsRevoked, true));

            await _userManager.UpdateSecurityStampAsync(user);

            return ServiceResult<bool>.Ok(true);
        }
    }
}
