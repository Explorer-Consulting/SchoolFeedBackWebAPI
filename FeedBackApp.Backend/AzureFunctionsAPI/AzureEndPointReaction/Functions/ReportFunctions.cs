using System.Net;
using Application.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions
{
    public sealed class ReportFunctions(IReportService reportService, ILogger<ReportFunctions> logger)
    {
        private readonly IReportService _reportService = reportService;
        private readonly ILogger<ReportFunctions> _reportLogger = logger;

        [Function("PerformReportCompilation")]
        public async Task<HttpResponseData> PerformReportCompilation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reports/{templateID}")]
            HttpRequestData request,
            string templateID)
        {
            Console.WriteLine($"Az elkuldott templateID az: {templateID}");
            try
            {
                await _reportService.CompileAndStore(templateID);

                var response = request.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(new
                {
                    reportId = templateID,
                    status = "Created"
                });
                return response;
            }
            catch (Exception ex)
            {
                _reportLogger.LogError(ex, "Error while compiling report {TemplateId}", templateID);

                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new
                {
                    error = "Report generation failed."
                });
                return response;
            }
        }
    }
}
