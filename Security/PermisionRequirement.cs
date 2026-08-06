using Core_Web.Services.Interfaces;
using Core_Web.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace Core_Web.Security
{
    public class PermissionRequirement : IAuthorizationRequirement { }

    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissions;
        private readonly IHttpContextAccessor _http;

        public PermissionHandler(IPermissionService permissions, IHttpContextAccessor http)
        {
            _permissions = permissions;
            _http = http;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var httpContext = _http.HttpContext;
            if (httpContext is null) return;

            var permiso = httpContext.GetEndpoint()?
                .Metadata
                .GetMetadata<RequiresPermissionAttribute>();

            if (permiso is null)
            {
                context.Succeed(requirement);
                return;
            }

            var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(idClaim, out var userId)) return;

            if (await _permissions.HasPermissionAsync(userId, permiso.Module, permiso.Action))
                context.Succeed(requirement);
        }
    }
}
