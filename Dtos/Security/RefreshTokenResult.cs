namespace Core_Web.Dtos.Security
{
    public record RefreshTokenResult(string RefreshToken,
        DateTime ExpiresIn
    )
    {
    }
}
