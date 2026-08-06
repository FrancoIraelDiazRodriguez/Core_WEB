using Core_Web.Dtos.Security;
using Core_Web.Enums;
using Core_Web.Models.Security;
using Core_Web.Services.Implementations;
using Core_Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Core_Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("login"), AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.LoginAsync(dto, ip);
            return result.Status switch
            {
                LoginStatus.Success => Ok(new { access_token = result.Token.AccessToken, token_type = "Bearer", expires_in = result.Token.ExpiresIn, refresh_token = result.RefreshToken!.RefreshToken }),
                LoginStatus.InvalidCredentials => Unauthorized(new { message = "Invalid credentials." }),
                LoginStatus.lockedOut => StatusCode(423, new { message = "Account is locked." }),
                LoginStatus.Inactive => StatusCode(403, new { message = "Account is inactive." }),
                LoginStatus.PasswordExpired => StatusCode(403, new { message = "Password has expired." }),
                _ => StatusCode(500, new { message = "An unexpected error occurred." })
            };
        }
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // ya es string
            var result = await _authService.ChangePasswordAsync(userId, dto);
            return result.IsSuccess ? NoContent() : BadRequest(new { errors = result.Errors });
        }
    }
}
