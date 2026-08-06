namespace Core_Web.Dtos.Security
{
    public record ChangePasswordDto(string CurrentPassword, string NewPassword)
    {
    }
}
