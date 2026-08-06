using Core_Web.Dtos.Security;
using Core_Web.Models.Security;

namespace Core_Web.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenResult> CreateRefreshTokenAsync(string userId, string ipAddress);
    }
}
