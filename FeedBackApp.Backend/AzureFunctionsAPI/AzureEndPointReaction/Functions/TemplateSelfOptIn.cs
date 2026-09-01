using System.Net;
using System.Security.Claims;
using ApplicationEventWorkers.SelfOptIn;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;

namespace ApplicationEventWorkers.AzureEndPointReaction.Functions;

/*
 * When a user is already authenticated,
 * this endpoint converts the preview link into actual access
 * by creating a questionnaire instance for that user. It is idempotent.
 * route: POST /api/templates/{id}/self-opt-in
 * Body: { "optInToken": "<jwt>" }
 * {id} = template Guid (same as in preview)
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

    private sealed class RequestDto { public string? OptInToken { get; set; } }

    [Function("TemplateSelfOptIn")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post",
         Route = "templates/{id}/self-opt-in")] HttpRequestData req,
        string id,
        FunctionContext context)   // StudentOnlyMiddleware should populate ctx.Items["User"]
    {
        // route Guid
        if (!Guid.TryParse(id, out var templateGuid))
            return await Text(req, HttpStatusCode.BadRequest, "Invalid template id (Guid expected).");

        // body with token
        var body = await req.ReadFromJsonAsync<RequestDto>() ?? new();
        if (string.IsNullOrWhiteSpace(body.OptInToken))
            return await Text(req, HttpStatusCode.Gone, "Missing opt-in token.");

        // validate token and bind to route
        var v = _tokens.Validate(body.OptInToken, DateTimeOffset.UtcNow);
        if (!v.IsValid)
            return await Text(req, HttpStatusCode.Gone, $"Invalid or expired link ({v.Error}).");

        if (v.QuestionnaireId != templateGuid)
            return await Text(req, HttpStatusCode.BadRequest, "Token/template mismatch.");

        // require authenticated user (middleware-enforced)
        if (!context.Items.TryGetValue("User", out var u) || u is not ClaimsPrincipal user)
            return req.CreateResponse(HttpStatusCode.Unauthorized);

        var email = user.FindFirst("email")?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email))
            return req.CreateResponse(HttpStatusCode.Unauthorized);

        // load real template by alias
        var template = await _db.Set<QuestionnaireTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template is null)
            return req.CreateResponse(HttpStatusCode.NotFound);

        if (!template.IsSelfOptInEnabled)
            return req.CreateResponse(HttpStatusCode.Forbidden);

        if (template.OptInExpiresAt is not null && template.OptInExpiresAt <= DateTimeOffset.UtcNow)
            return await Text(req, HttpStatusCode.Gone, "Self opt-in window has closed.");

        // optional: capacity check per template
        if (template.MaxParticipants is int max && max >= 0)
        {
            var current = await _db.Set<Questionnaire>()
                .CountAsync(q => q.SurveyId == template.Id); // link via stored template Id
            if (current >= max)
                return await Text(req, HttpStatusCode.Forbidden, "Capacity reached for this template.");
        }

        // idempotency: one questionnaire per (StudentEmail, Template)
        var exists = await _db.Set<Questionnaire>()
            .AnyAsync(q => q.SurveyId == template.Id && q.StudentEmail == email);

        if (exists)
        {
            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(new { status = "already_has_access" });
            return ok;
        }

        // create a real Questionnaire instance (minimal fields; results will be filled on submit)
        var instance = new Questionnaire
        {
            Id = Guid.NewGuid().ToString("D"),
            Status = false,               // not completed
            SurveyId = template.Id,       // link instance to template via stored Id (questiontemplates_<guid>)
            TeacherEmail = string.Empty,  // unknown in self-opt-in path
            StudentEmail = email,
            SubjectName = string.Empty,
            QuestionnaireResults = new List<QuestionAnswer>() // keep empty; filled when answering
        };

        _db.Add(instance);
        await _db.SaveChangesAsync();

        var created = req.CreateResponse(HttpStatusCode.Created);
        await created.WriteAsJsonAsync(new { status = "granted", questionnaireId = instance.Id });
        return created;
    }

    private static async Task<HttpResponseData> Text(HttpRequestData req, HttpStatusCode code, string message)
    {
        var res = req.CreateResponse(code);
        await res.WriteStringAsync(message);
        return res;
    }
}
