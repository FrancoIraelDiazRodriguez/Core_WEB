using Core_Web.Dtos.Security;

namespace Core_Web.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginDto loginDto, string? ipAddress);
        Task<ChangePasswordResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
    }
}
