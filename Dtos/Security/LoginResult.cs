using Core_Web.Enums;

namespace Core_Web.Dtos.Security
{
    public record LoginResult(LoginStatus Status, TokenResult? Token = null, RefreshTokenResult? RefreshToken = null)
    {
       
    }
}
