using Core_Web.Models.Security;
using Core_Web.Dtos.Security;
namespace Core_Web.Services.Interfaces
{
    public interface ITokenService
    {
        TokenResult CreateToken(ApplicationUser user, IList<string> roles, bool isMobile = false);
        string GenerateRefreshToken();

        string HashToken(string token);
    }
}
