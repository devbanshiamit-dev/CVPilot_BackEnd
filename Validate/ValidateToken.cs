using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CVPilotAPI.Validate
{
    public class ValidateToken
    {
        private readonly IConfiguration _configuration;
        public ValidateToken(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public TokenValidationParameters CreateValidationParameters()
        {
            var key = _configuration["JWT:ServerKey"]!;
            var issuer = _configuration["JWT:Issuer"]!;
            var audience = _configuration["JWT:Audience"]!;

            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }
        public ClaimsPrincipal? ValidateAccessToken(
            string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(
                    accessToken,
                    CreateValidationParameters(),
                    out var validatedToken
                );

                if (validatedToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
