using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;

namespace FeedBackApp.Backend.Infrastructure.Middleware
{
    public class StudentOnlyMiddleware : IFunctionsWorkerMiddleware
    {
        private const string JwtCookieName = "token"; // Name of the cookie containing the token

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
            if (!JwtRoleValidator.IsStudent(token,context))
            {
                await ReturnForbidden.ExecuteAsync(context, httpRequestData, "Student privilages required!");
                return;
            }

            await next(context);
        }
    }
}
