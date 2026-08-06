namespace Core_Web.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(long userId, string module, string action);
    }
}
