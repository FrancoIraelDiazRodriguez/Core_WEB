using Core_Web.Data;
using Core_Web.Dtos.Security;
using Core_Web.Models.Security;
using Core_Web.Services.Interfaces;

namespace Core_Web.Services.Implementations
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly CoreContext _context;
        private readonly ITokenService _tokenService;

        public RefreshTokenService(CoreContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<RefreshTokenResult> CreateRefreshTokenAsync(string userId, string ipAddress)
        {
            var plain = _tokenService.GenerateRefreshToken();
            var hash = _tokenService.HashToken(plain);
            var expiresAt = DateTime.UtcNow.AddDays(7); // 7 días web (el móvil de 30 lo vemos luego)

            var result = new RefreshToken
            {
                Token = hash,
                UserId = long.Parse(userId),
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            _context.RefreshTokens.Add(result);
            await _context.SaveChangesAsync();

            return new RefreshTokenResult(plain, expiresAt);
            
        }
    }
}
