using FeedBackApp.Backend.Infrastructure.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FeedBackApp.Backend.Infrastructure.Middleware.Utils
{
    public class JwtRoleValidator
    {
        private readonly IOptions<JwtOptions> _jwtOptions;

        public JwtRoleValidator(IOptions<JwtOptions> jwtOptions) => _jwtOptions = jwtOptions;
        private ClaimsPrincipal? ValidateToken(string token)
        {
            var secretKey = _jwtOptions.Value.SecretKey;
            if (string.IsNullOrEmpty(secretKey))
                return null;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtOptions.Value.Issuer,
                    ValidAudience = _jwtOptions.Value.Audience,
                    IssuerSigningKey = key,
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

        public bool HasRole(string token, string role, FunctionContext? context = null)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return false;

            if (context != null)
                context.Items["User"] = principal;

            var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            return roleClaim?.Value == role;
        }

        public bool IsAdmin(string token, FunctionContext? context = null) => HasRole(token, "Admin", context);
        public bool IsStudent(string token, FunctionContext? context = null) => HasRole(token, "Student", context);
    }
}