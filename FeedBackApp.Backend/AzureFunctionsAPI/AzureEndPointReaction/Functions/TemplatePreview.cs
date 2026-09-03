using System.Net;
using ApplicationEventWorkers.SelfOptIn;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace ApplicationEventWorkers.AzureEndPointReaction.Functions;

/*
 * Serve a read-only questionnaire preview to anyone holding a valid self-opt-in link.
 * It never mutates data.
 * route: GET /api/templates/{id}/preview?optin=<jwt>
 * {id} = ULID of the questionnaire template
 *    (the public alias backfilled into QuestionnaireTemplate.TemplateUlid via "DebugBackfillTemplateUlids")
 * optin = short-lived JWT created by IOptInTokenService
 *    (purpose "optin", contains tid claim = same ULID).
 */

public class TemplatePreview
{
    private readonly IOptInTokenService _tokens;
    private readonly AppDBContext _db;

    public TemplatePreview(IOptInTokenService tokens, AppDBContext db)
    {
        _tokens = tokens;
        _db = db;
    }

    [Function("TemplatePreview")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
         Route = "templates/{id}/preview")] HttpRequestData req,
        string id)
    {   
        // 1) id must be Guid
        if (!Guid.TryParse(id, out var templateId))
            return await Text(req, HttpStatusCode.BadRequest, "Invalid template id (Guid expected).");

        // 2) validate token
        var qs = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var optinToken = qs.Get("optin");
        if (string.IsNullOrWhiteSpace(optinToken))
            return await Text(req, HttpStatusCode.Gone, "Missing opt-in token.");

        var v = _tokens.Validate(optinToken, DateTimeOffset.UtcNow);
        if (!v.IsValid)
            return await Text(req, HttpStatusCode.Gone, $"Invalid or expired link ({v.Error}).");

        if (v.QuestionnaireId != templateId)
            return await Text(req, HttpStatusCode.BadRequest, "Token/template mismatch.");

        // 3) load template by alias (Guid)
        var storageId = $"questiontemplates_{templateId:D}";
        
        var template = await _db.Set<QuestionnaireTemplate>()
            .AsNoTracking()
            .Where(t => EF.Property<string>(t, "DocumentType") == "QuestionTemplate")
            .SingleOrDefaultAsync(t => t.Id == storageId);

        
        if (template is null)
        {
            var nf = req.CreateResponse(HttpStatusCode.NotFound);
            await nf.WriteStringAsync($"Template not found for Guid '{id}'");
            return nf;
        }

        
        if (!template.IsSelfOptInEnabled)
            return await Text(req, HttpStatusCode.Forbidden, "Self opt-in disabled for this template.");


        if (template.OptInExpiresAt is not null && template.OptInExpiresAt <= DateTimeOffset.UtcNow)
            return await Text(req, HttpStatusCode.Gone, "Self opt-in window has closed.");

        var payload = new TemplatePreviewDto
        {
            Id = id,
            Title = template.Title ?? string.Empty,
            SelfEnrollmentAllowed = template.IsSelfOptInEnabled
        };


        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(payload);
        return ok;
    }

    private static async Task<HttpResponseData> Text(HttpRequestData req, HttpStatusCode code, string message)
    {
        var res = req.CreateResponse(code);
        await res.WriteStringAsync(message);
        return res;
    }

    private sealed class TemplatePreviewDto
    {
        public string Id { get; set; } = default!;
        public string Title { get; set; } = default!;
        public bool SelfEnrollmentAllowed { get; set; }
    }    
    
    // Admin functions, end user will not see these
    // 1.) to list all the templates and ids
    [Function("DebugListTemplates")]
    public async Task<HttpResponseData> DebugListTemplates(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "debug/templates")]
        HttpRequestData req)
    {
        var data = await _db.Set<QuestionnaireTemplate>()
            .AsNoTracking()
            .Select(t => new { t.Id })
            .Take(50)
            .ToListAsync();

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { count = data.Count, data });
        return res;
    }
    
    // 2.) turn opt-in on & off
    [RequireAdmin]
    [Function("DebugEnableOptIn")]
    public async Task<HttpResponseData> DebugEnableOptIn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "debug/templates/{guid}/enable-optin")]
        HttpRequestData req, string guid)
    {
        var storageId = $"questiontemplates_{guid:D}";

        var t = await _db.Set<QuestionnaireTemplate>()
            .Where(t => EF.Property<string>(t, "DocumentType") == "QuestionTemplate")
            .SingleOrDefaultAsync(t => t.Id == storageId);

        if (t is null) return req.CreateResponse(HttpStatusCode.NotFound);

        t.IsSelfOptInEnabled = true;

        // set a future expiry if none or already past
        if (t.OptInExpiresAt is null || t.OptInExpiresAt <= DateTimeOffset.UtcNow)
            t.OptInExpiresAt = DateTimeOffset.UtcNow.AddDays(7);

        await _db.SaveChangesAsync();

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(new { guid, t.IsSelfOptInEnabled, t.OptInExpiresAt });
        return ok;
    }

}
