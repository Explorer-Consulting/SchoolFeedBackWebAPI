using System.Net;
using ApplicationEventWorkers.SelfOptIn;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
/*
 * A simple HTTP GET function to generate a shareable opt-in link for testing/admin usage.
 * Lets us verify end-to-end that token creation works
 * and that the host/DI/config is correct before building the Preview/Opt-In endpoints
 * note: used optin/... to avoid conflicts with Functions built-in admin routes.
 * Parses query string: tid, optional tag, optional minutes for TTL.
 * Calls IOptInTokenService.CreateToken() to mint a JWT { tid, tag, exp }.
 * Creates new URL
 * Returns JSON
 * 
 */

namespace ApplicationEventWorkers.AzureEndPointReaction.Functions;

public class AdminShareLink
{
    private readonly IOptInTokenService _tokens;
    public AdminShareLink(IOptInTokenService tokens) => _tokens = tokens;

    [Function("ShareOptInLink")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
            Route = "optin/share-link/{tid}")] HttpRequestData req, string tid)
    {
        
        if (!Guid.TryParse(tid, out var templateId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing/invalid tid (expected Guid).");
            return bad;
        }

        var qs = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var minutes = int.TryParse(qs.Get("minutes"), out var m) ? m : 60;
        var tag = qs.Get("tag") ?? "@template-id";
        var exp = DateTimeOffset.UtcNow.AddMinutes(minutes);

        var token = _tokens.CreateToken(templateId, tag, exp);
        var frontendUrl = Environment.GetEnvironmentVariable("FrontendUrl");
        var url = $"{frontendUrl}/questionnairetemplate/{tid}/preview?optin={Uri.EscapeDataString(token)}";
        // templates/tid nem kell
        // endpoint

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(new { url, expiresAt = exp });
        return ok;
    }

}
