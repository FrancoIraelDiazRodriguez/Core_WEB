namespace Core_Web.Dtos.Security
{
    public record TokenResult(
        string AccessToken,
        int ExpiresIn
    );
}
