using Application.Extensions.QuestionnaireExtensions;
using FeedBackApp.Core.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ApplicationEventWorkers.AzureEndPointReaction.Functions
{
    /// <summary>
    /// Handles HTTP requests to preview a questionnaire template by its identifier.
    /// </summary>
    /// 
    /*
     * this is only for constructing the feature, without being exhaustive on error handling or security
     */
    public sealed class DisplayQuestionnaireTemplateEvent(
        ILogger<DisplayQuestionnaireTemplateEvent> logger,
        IEvaluationRepository repository)
    {
        [Function(nameof(DisplayQuestionnaireTemplateEvent))]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "questionnairetemplate/{id}/preview")]
            HttpRequestData request,
            string id)
        {
            logger.LogInformation("DisplayQuestionnaireTemplate processed request for ID {Id}", id);

            try
            {
                var template = await repository.GetQuestionTemplateBySurveyIdAsync(id);

                if (template is null)
                {
                    var notFound = request.CreateResponse(HttpStatusCode.NotFound);
                    await notFound.WriteStringAsync($"No questionnaire template found for ID: {id}");
                    return notFound;
                }

                var previewDto = template.ToPreviewDto();

                var response = request.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(previewDto);
                return response;
            }
            catch (Exception ex) //for development only, when all of the features will be completed, we will handle specific exceptions
            {
                logger.LogError(ex, "Error processing request for ID {Id}", id);

                var errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while processing your request.");
                return errorResponse;
            }
        }
    }
}
