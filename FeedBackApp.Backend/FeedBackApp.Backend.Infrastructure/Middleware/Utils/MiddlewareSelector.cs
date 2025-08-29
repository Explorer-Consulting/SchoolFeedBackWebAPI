using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace FeedBackApp.Backend.Infrastructure.Middleware.Utils;
public class MiddlewareSelector : IFunctionsWorkerMiddleware
{
    private readonly AdminOnlyMiddleware _adminOnly;
    private readonly StudentOnlyMiddleware _studentOnly;

    public MiddlewareSelector(AdminOnlyMiddleware adminOnly, StudentOnlyMiddleware studentOnly)
    {
        _adminOnly = adminOnly;
        _studentOnly = studentOnly;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var entryPoint = context.FunctionDefinition.EntryPoint;
        var lastDot = entryPoint.LastIndexOf('.');
        var className = entryPoint[..lastDot];
        var methodName = entryPoint[(lastDot + 1)..];

        // Try to get the type from the executing assembly first
        var type = Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(t => t.FullName == className);

        // If not found, try all loaded assemblies
        if (type == null)
        {
            type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == className);
        }

        // Log if type is still not found
        if (type == null)
        {
            context.GetLogger<MiddlewareSelector>().LogError($"Type not found: {className}");
            await next(context);
            return;
        }

        var method = type.GetMethod(methodName);
        var httpRequestData = await context.GetHttpRequestDataAsync();
        if (httpRequestData == null)
        {
            // This middleware won't run for non-HTTP triggers
            await next(context);
            return;
        }

        Console.WriteLine($"Function EntryPoint: {entryPoint}");
        Console.WriteLine($"Resolved Type: {type}");
        Console.WriteLine($"Resolved Method: {method}");
        Console.WriteLine($"Attributes: {string.Join(", ", method?.GetCustomAttributes().Select(a => a.GetType().Name) ?? Array.Empty<string>())}");



        if (method != null)
        {
            if (method.GetCustomAttribute<RequireAdminAttribute>() != null)
            {
                await _adminOnly.Invoke(context, next);
                return;
            }

            if (method.GetCustomAttribute<RequireStudentAttribute>() != null)
            {
                await _studentOnly.Invoke(context, next);
                return;
            }
        }

        // No role attribute — just continue
        await next(context);
    }
}
