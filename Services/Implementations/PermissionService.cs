using Core_Web.Data;
using Core_Web.Models.Security;
using Core_Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core_Web.Services.Implementations
{
    public class PermissionService(CoreContext context) : IPermissionService
    {
    

        private readonly CoreContext _context = context;

        public async Task<bool> HasPermissionAsync(long userId, string module, string action)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
                .Where(r => r.IsActive)
                .SelectMany(r => r.Routes)
                .AnyAsync(route => route.IsActive
                                && route.Module == module
                                && route.Action == action);
        }

    }
}
