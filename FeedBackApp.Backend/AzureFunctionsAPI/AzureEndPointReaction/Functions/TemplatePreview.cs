using System.Net;
using ApplicationEventWorkers.SelfOptIn;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using NUlid;

namespace ApplicationEventWorkers.AzureEndPointReaction.Functions;

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
        // route id must be a ULID
        if (!Ulid.TryParse(id, out var templateUlid))
            return await Text(req, HttpStatusCode.BadRequest, "Invalid template id (ULID expected).");

        // extract and validate token
        var qs = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var optinToken = qs.Get("optin");
        if (string.IsNullOrWhiteSpace(optinToken))
            return await Text(req, HttpStatusCode.Gone, "Missing opt-in token.");

        var v = _tokens.Validate(optinToken, DateTimeOffset.UtcNow);
        if (!v.IsValid)
            return await Text(req, HttpStatusCode.Gone, $"Invalid or expired link ({v.Error}).");

        // tken's ULID must match the route id
        if (v.QuestionnaireId != templateUlid)
            return await Text(req, HttpStatusCode.BadRequest, "Token/template mismatch.");

        // load template by storage id ("questiontemplates_{ulid}")
        var storageId = $"questiontemplates_{id}";
        var template = await _db.Set<QuestionnaireTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateUlid == id);  // 'id' is ULID from route
        
        if (template is null)
        {
            var nf = req.CreateResponse(HttpStatusCode.NotFound);
            await nf.WriteStringAsync($"Template not found: expected id 'questiontemplates_{id}'");
            return nf;
        }

        //
        // if (template is null)
        //     return req.CreateResponse(HttpStatusCode.NotFound);

        if (!template.IsSelfOptInEnabled)
            return req.CreateResponse(HttpStatusCode.Forbidden);

        if (template.OptInExpiresAt is not null && template.OptInExpiresAt <= DateTimeOffset.UtcNow)
            return await Text(req, HttpStatusCode.Gone, "Self opt-in window has closed.");

        // TODO capacity
        int? capacityLeft = template.MaxParticipants;

        // build sanitized read-only response
        var payload = new TemplatePreviewDto
        {
            Id = id,
            Title = template.Title,
            Questions = template.QuestionTemplates.Select(q => new QuestionPreviewDto
            {
                Id = q.Id,
                Question = q.Question,
                Type = q.Type.ToString(),
                AnswerOptions = q.AnswerOptions.ToArray(),
                Category = q.Category,
                Description = q.Description
            }).ToArray(),
            OptIn = new TemplateOptInInfo
            {
                Enabled = true,
                ExpiresAt = v.ExpiresAtUtc,
                CapacityLeft = capacityLeft
            }
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

    // response DTOs (local to function)
    private sealed class TemplatePreviewDto
    {
        public string Id { get; set; } = default!;
        public string Title { get; set; } = default!;
        public QuestionPreviewDto[] Questions { get; set; } = Array.Empty<QuestionPreviewDto>();
        public TemplateOptInInfo OptIn { get; set; } = new();
    }

    private sealed class QuestionPreviewDto
    {
        public string Id { get; set; } = default!;
        public string Question { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string[] AnswerOptions { get; set; } = Array.Empty<string>();
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private sealed class TemplateOptInInfo
    {
        public bool Enabled { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public int? CapacityLeft { get; set; }
    }
    
    [Function("DebugListTemplates")]
    public async Task<HttpResponseData> DebugListTemplates(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "debug/templates")] HttpRequestData req)
    {
        var ids = await _db.Set<QuestionnaireTemplate>()
            .AsNoTracking()
            .Select(t => t.Id)
            .Take(50)
            .ToListAsync();

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { count = ids.Count, ids });
        return res;
    }

}
