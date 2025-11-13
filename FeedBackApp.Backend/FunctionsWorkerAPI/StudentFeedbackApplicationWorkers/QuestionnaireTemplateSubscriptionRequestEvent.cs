using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class QuestionnaireTemplateSubscriptionRequestEvent
    {
        [Function(nameof(QuestionnaireTemplateSubscriptionRequestEvent))]
        [OpenApiOperation(
            operationId: "CreateQuestionnaireTemplateSubscription",
            tags: ["Questionnaire Templates"],
            Summary = "Subscribe to a questionnaire template",
            Description = "Creates a new subscription for the specified questionnaire template.")]
        [OpenApiParameter(
            name: "questionnaire-template-id",
            In = ParameterLocation.Path,
            Required = true,
            Type = typeof(string),
            Summary = "Unique identifier of the questionnaire template",
            Description = "Path parameter of v1/questionnaire-templates/{questionnaire-template-id}.")]
        [OpenApiRequestBody(
            contentType: "application/json",
            bodyType: typeof(object),
            Required = true,
            Description = "Subscription details in raw JSON.")]
        [OpenApiResponseWithBody(
            HttpStatusCode.Created,
            "application/json",
            typeof(object),
            Summary = "Created",
            Description = "Subscription successfully created.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.BadRequest,
            Summary = "Bad Request",
            Description = "Missing or invalid input.")]
        [OpenApiResponseWithoutBody(
            HttpStatusCode.Conflict,
            Summary = "Conflict",
            Description = "User already subscribed.")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/questionnaire-templates/{questionnaire-template-id}/subscriptions")]
            HttpRequestData request,
            Guid questionnaireTemplateId)
        {
            var body = await request.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                var bad = request.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Missing JSON body.");
                return bad;
            }

            var created = request.CreateResponse(HttpStatusCode.Created);
            created.Headers.Add("Location", $"/api/v1/questionnaire-templates/{questionnaireTemplateId}/subscriptions/placeholder-id");
            await created.WriteStringAsync("{\"message\":\"Subscription created.\"}");
            return created;
        }
    }
}
