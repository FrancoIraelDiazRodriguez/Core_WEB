namespace Core_Web.Dtos.Security
{
    public record ChangePasswordResult(bool IsSuccess, IEnumerable<string> Errors)
    {
    }
}
