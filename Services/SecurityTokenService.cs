using EmployeeManagementSystem.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagementSystem.Services
{
    public class SecurityTokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly IMemoryCache _cache;

        public SecurityTokenService(IConfiguration config, IMemoryCache cache)
        {
            var jwtSettingsSection = config.GetSection("JwtSettings");
            _key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettingsSection["SuperSecretKey"] ?? throw new ArgumentNullException("SuperSecretKey configuration is missing.")));
            _issuer = jwtSettingsSection["Issuer"] ?? throw new ArgumentNullException("Issuer configuration is missing.");
            _cache = cache;
        }

        public async Task<SecurityTokenResponse> GenerateSecurityTokenAsync(AppUser user)
        {
            // Check if the token is already cached
            if (_cache.TryGetValue(user.Id, out SecurityTokenResponse cachedToken))
            {
                return cachedToken;
            }
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.UserName),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                IssuedAt = DateTime.UtcNow,
                Issuer = _issuer,
                SigningCredentials = creds
            };
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);
            // Cache the generated token with an expiration time of 1 day
            _cache.Set(user.Id, securityToken, securityToken.ValidTo);
            return new SecurityTokenResponse { UserName = user.UserName, Token = tokenString, Expiration = securityToken.ValidTo };
        }
    }
}
