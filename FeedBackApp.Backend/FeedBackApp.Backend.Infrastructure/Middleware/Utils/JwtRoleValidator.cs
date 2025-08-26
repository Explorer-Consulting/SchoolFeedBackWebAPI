using Microsoft.Azure.Functions.Worker;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FeedBackApp.Backend.Infrastructure.Middleware.Utils
{
    public static class JwtRoleValidator
    {
        private static ClaimsPrincipal? ValidateToken(string token)
        {
            var secretKey = Environment.GetEnvironmentVariable("JwtSecretKey");
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
                    ValidIssuer = "SchoolFeedbackWebAPI",
                    ValidAudience = "SchoolFeedbackWebAPI",
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

        public static bool HasRole(string token, string role, FunctionContext? context = null)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return false;

            if (context != null)
                context.Items["User"] = principal;

            var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            return roleClaim?.Value == role;
        }

        public static bool IsAdmin(string token, FunctionContext? context = null) => HasRole(token, "Admin", context);
        public static bool IsStudent(string token, FunctionContext? context = null) => HasRole(token, "Student", context);
    }
}