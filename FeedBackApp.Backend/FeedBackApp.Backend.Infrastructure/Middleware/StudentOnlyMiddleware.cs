using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace FeedBackApp.Backend.Infrastructure.Middleware
{
    public class StudentOnlyMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly JwtRoleValidator _jwtRoleValidator;
        private const string JwtCookieName = "token"; // Name of the cookie containing the token

        public StudentOnlyMiddleware(JwtRoleValidator jwtRoleValidator)
        {
            _jwtRoleValidator = jwtRoleValidator;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpRequestData = await context.GetHttpRequestDataAsync();

            if (httpRequestData == null)
            {
                await next(context);
                return;
            }

            // Look for the cookie manually
            var tokenCookie = httpRequestData.Cookies.FirstOrDefault(c => c.Name == JwtCookieName);
            if (tokenCookie == null || string.IsNullOrWhiteSpace(tokenCookie.Value))
            {
                await ReturnForbidden.ExecuteAsync(context, httpRequestData, "Cookie not provided in the request");
                return;
            }

            var token = tokenCookie.Value;

            // Validate the token
            if (!_jwtRoleValidator.IsStudent(token, context))
            {
                await ReturnForbidden.ExecuteAsync(context, httpRequestData, "Student privilages required!");
                return;
            }

            await next(context);
        }
    }
}
