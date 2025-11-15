using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace FunctionsWorkerAPI.StudentFeedbackApplicationWorkers
{
    public sealed class SubscriberRegistrationRequestEvent(ILogger<SubscriberRegistrationRequestEvent> logger)
    {
        [Function("SignInWithGoogle")]
        [OpenApiOperation(operationId: "SignInWithGoogle", tags: ["Authentication"], Summary = "Sign in with Google")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Summary = "Authenticated")]
        public static async Task<HttpResponseData> SignInWithGoogleAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/sign-in/google")]
            HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Signed in with Google.");
            return response;
        }

        [Function("SignInWithFacebook")]
        [OpenApiOperation(operationId: "SignInWithFacebook", tags: ["Authentication"], Summary = "Sign in with Facebook")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Summary = "Authenticated")]
        public static async Task<HttpResponseData> SignInWithFacebookAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/sign-in/facebook")]
            HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Signed in with Facebook.");
            return response;
        }

        [Function("SignInWithLinkedln")]
        [OpenApiOperation(operationId: "SignInWithLinkedIn", tags: ["Authentication"], Summary = "Sign in with LinkedIn")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Summary = "Authenticated")]
        public static async Task<HttpResponseData> SignInWithLinkedInAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/sign-in/linkedln")]
            HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Signed in with LinkedIn.");
            return response;
        }

        [Function("SignInWithMicrosoft")]
        [OpenApiOperation(operationId: "SignInWithMicrosoft", tags: ["Authentication"], Summary = "Sign in with Microsoft")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Summary = "Authenticated")]
        public static async Task<HttpResponseData> SignInWithMicrosoftAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/sign-in/microsoft")]
            HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Signed in with Microsoft.");
            return response;
        }

        [Function("SignInPasswordless")]
        [OpenApiOperation(operationId: "SignInPasswordless", tags: ["Authentication"], Summary = "Passwordless sign in")]
        [OpenApiRequestBody("application/json", typeof(object), Required = false, Description = "Optional payload (e.g., email for magic link)")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Summary = "Authenticated or link sent")]
        public static async Task<HttpResponseData> SignInPasswordlessAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "v1/sign-in/passwordless")]
            HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Passwordless sign-in processed.");
            return response;
        }
    }
}
