using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureFunctionsAPI.AzureEndPointReaction.Middleware
{
    public class ValidationInvocationFilter : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<ValidationInvocationFilter> _logger;

        public ValidationInvocationFilter(ILogger<ValidationInvocationFilter> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            await next(context);
        }
    }
}

