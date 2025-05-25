using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NRI.Classes;

namespace NRI.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryHours;

        public JwtService(IConfiguration config)
        {
            _secret = config["Jwt:Secret"] ??
                throw new ArgumentNullException("Jwt:Secret не настроен в конфигурации");
            _issuer = config["Jwt:Issuer"] ?? "NRI_App";
            _audience = config["Jwt:Audience"] ?? "NRI_Client";
            _expiryHours = config.GetValue<int>("Jwt:ExpiryHours", 24);
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

            // Получаем роли из связи UserRoles
            var roles = user.UserRoles?.Select(ur => ur.Role?.RoleName) ?? new List<string?> { "Игрок" };
            var roleClaims = roles.Where(r => r != null).Select(r => new Claim(ClaimTypes.Role, r!));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("exp", new DateTimeOffset(DateTime.UtcNow.AddHours(_expiryHours)).ToUnixTimeSeconds().ToString())
            };

            claims.AddRange(roleClaims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_expiryHours),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public bool TryValidateToken(string token, out ClaimsPrincipal principal)
        {
            principal = ValidateToken(token);
            return principal != null;
        }
        public bool TryParseUserIdFromToken(string token, out int userId)
        {
            userId = 0;
            try
            {
                var principal = ValidateToken(token);
                var claim = principal?.FindFirst(ClaimTypes.NameIdentifier);
                return claim != null && int.TryParse(claim.Value, out userId);
            }
            catch
            {
                return false;
            }
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public static string GenerateJwtSecret()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] key = new byte[32]; 
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }
    }
}
