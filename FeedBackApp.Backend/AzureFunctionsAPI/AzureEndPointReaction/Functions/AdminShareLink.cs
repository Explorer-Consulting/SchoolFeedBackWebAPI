using System.Net;
using ApplicationEventWorkers.SelfOptIn;
using FeedBackApp.Backend.Infrastructure.Configuration;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    private readonly IOptions<SelfOptInJwtOptions> _options;
    private readonly IOptions<FrontendOptions> _frontendOptions;
    public AdminShareLink(IOptInTokenService tokens, IOptions<SelfOptInJwtOptions> options,
        IOptions<FrontendOptions> frontendOptions)
    {
        _tokens = tokens; 
        _options = options;
        _frontendOptions = frontendOptions;
    }

    [RequireAdmin]
    [Function("ShareOptInLink")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
            Route = "optin/share-link/{tid}")] HttpRequestData req, string tid)
    {

        // checking if self-opt in is enabled in configuration
        if (!_options.Value.Enabled)
        {
            var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
            await forbidden.WriteStringAsync("Self opt-in is not enabled on this deployment");
            return forbidden;
        }

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
        var frontendUrl = _frontendOptions.Value.Url;
        var url = $"{frontendUrl}/questionnairetemplate/{tid}/preview?optin={Uri.EscapeDataString(token)}";
        // templates/tid nem kell
        // endpoint

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(new { url, expiresAt = exp });
        return ok;
    }

}
