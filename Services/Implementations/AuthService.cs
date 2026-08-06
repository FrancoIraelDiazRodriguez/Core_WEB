using Core_Web.Dtos.Security;
using Core_Web.Enums;
using Core_Web.Models.Security;
using Core_Web.Services.Interfaces;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Core_Web.Services.Implementations
{
    public class AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IRefreshTokenService refreshTokenService) : IAuthService
    {
        private RefreshTokenResult? refreshToken;

        public UserManager<ApplicationUser> UserManager { get; } = userManager;
        public SignInManager<ApplicationUser> SignInManager { get; } = signInManager;
        public ITokenService TokenService { get; } = tokenService;
        public IRefreshTokenService RefreshTokenService { get; } = refreshTokenService;

        public async Task<LoginResult> LoginAsync(LoginDto dto, string? ipAddress)
        {
            var user = await UserManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return new LoginResult(LoginStatus.InvalidCredentials, Token: null, RefreshToken: null);

            var result = await SignInManager
                .CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return new LoginResult(LoginStatus.lockedOut, null, null);

            if (!result.Succeeded)
                return new LoginResult(LoginStatus.InvalidCredentials, null, null);

            if (!user.IsActive)
                return new LoginResult(LoginStatus.Inactive, null, null);

            if (user.PasswordExpireAt is not null && user.PasswordExpireAt < DateTime.UtcNow)
                return new LoginResult(LoginStatus.PasswordExpired, null, null);

            var roles = await UserManager.GetRolesAsync(user);
            var token = TokenService.CreateToken(user, roles);
            var refresh = await refreshTokenService.CreateRefreshTokenAsync(user.Id.ToString(), ipAddress);
            return new LoginResult(LoginStatus.Success, token, refresh);
        }

        public async Task<ChangePasswordResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user is null)
                return new ChangePasswordResult(false, ["User not found."]);
            var result = await UserManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return new ChangePasswordResult(false, [string.Join(", ", result.Errors.Select(e => e.Description))]);
            // Update password expiration date
            user.PasswordExpireAt = DateTime.UtcNow.AddDays(90); // Example: Password expires in 90 days
            await UserManager.UpdateAsync(user);
            return new ChangePasswordResult(true, ["Password changed successfully."]);
        }
    }
}
