using System.Net;
using System.Security.Claims;
using ApplicationEventWorkers.SelfOptIn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using NUlid;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;

namespace ApplicationEventWorkers.AzureEndPointReaction.Functions;

/*
 * When a user is already authenticated,
 * this endpoint converts the preview link into actual access
 * by creating a questionnaire instance for that user. It is idempotent.
 * route: POST /api/templates/{id}/self-opt-in
 * Body: { "optInToken": "<jwt>" }
 * {id} = template ULID (same as in preview)
 * optInToken = the same short-lived JWT from the share link.
 */

public class TemplateSelfOptIn
{
    private readonly IOptInTokenService _tokens;
    private readonly AppDBContext _db;

    public TemplateSelfOptIn(IOptInTokenService tokens, AppDBContext db)
    {
        _tokens = tokens;
        _db = db;
    }

    public sealed class RequestDto { public string? OptInToken { get; set; } }

    [Function("TemplateSelfOptIn")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post",
         Route = "templates/{id}/self-opt-in")] HttpRequestData req,
        string id,
        FunctionContext context)   // to read identity from middleware
    {
        if (!Ulid.TryParse(id, out var templateId))
            return await Text(req, HttpStatusCode.BadRequest, "Invalid template id (ULID expected).");

        var body = await req.ReadFromJsonAsync<RequestDto>() ?? new();
        if (string.IsNullOrWhiteSpace(body.OptInToken))
            return await Text(req, HttpStatusCode.Gone, "Missing opt-in token.");

        var v = _tokens.Validate(body.OptInToken, DateTimeOffset.UtcNow);
        if (!v.IsValid)
            return await Text(req, HttpStatusCode.Gone, $"Invalid or expired link ({v.Error}).");

        if (v.QuestionnaireId != templateId)
            return await Text(req, HttpStatusCode.BadRequest, "Token/template mismatch.");

        // require authenticated user (via StudentOnlyMiddleware)
        ClaimsPrincipal? user = null;
        if (context.Items.TryGetValue("User", out var u) && u is ClaimsPrincipal cp) user = cp;
        if (user is null) return req.CreateResponse(HttpStatusCode.Unauthorized);

        var email = user.FindFirst("email")?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email))
            return req.CreateResponse(HttpStatusCode.Unauthorized);

        // load template
        var template = await _db.Set<SurveyTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId.ToString());

        if (template is null)
            return req.CreateResponse(HttpStatusCode.NotFound);

        if (!template.IsSelfOptInEnabled)
            return req.CreateResponse(HttpStatusCode.Forbidden);

        if (template.OptInExpiresAt is not null && template.OptInExpiresAt <= DateTimeOffset.UtcNow)
            return await Text(req, HttpStatusCode.Gone, "Self opt-in window has closed.");

        // creation of a Questionnaire instance for this user
        // build a deterministic or new ULID instance id; here we use a new ULID.
        var instanceId = Ulid.NewUlid().ToString();

        // check if already exists
        var already = await _db.Set<Questionnaire>()
            .AnyAsync(q => q.TemplateId == templateId.ToString() && q.StudentEmail == email);

        if (already)
        {
            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(new { status = "already_has_access" });
            return ok;
        }

        var now = DateTimeOffset.UtcNow;

        var q = new Questionnaire
        {
            Id = instanceId,
            TemplateId = templateId.ToString(),
            StudentEmail = email,
            CreatedAt = now,
            AccessType = "SelfOptIn",
            Title = template.Title,
            Description = template.Description,
            Questions = template.Questions // shallow copy is fine for read-only questions
        };

        _db.Add(q);
        await _db.SaveChangesAsync();

        var created = req.CreateResponse(HttpStatusCode.Created);
        await created.WriteAsJsonAsync(new { status = "granted", questionnaireId = instanceId });
        return created;
    }

    private static async Task<HttpResponseData> Text(HttpRequestData req, HttpStatusCode code, string message)
    {
        var res = req.CreateResponse(code);
        await res.WriteStringAsync(message);
        return res;
    }
    
    // TEMPORARY - will replace with QuestionnaireTemplate and the 'actual' Questionnaire class

    private sealed class SurveyTemplate
    {
        public string Id { get; init; } = default!;
        public string Title { get; init; } = default!;
        public string Description { get; init; } = default!;
        public List<string> Questions { get; init; } = new();
        public bool IsSelfOptInEnabled { get; init; }
        public DateTimeOffset? OptInExpiresAt { get; init; }
    }

    private sealed class Questionnaire
    {
        public string Id { get; set; } = default!;
        public string TemplateId { get; set; } = default!;
        public string StudentEmail { get; set; } = default!;
        public string AccessType { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public List<string> Questions { get; set; } = new();
    }
}
