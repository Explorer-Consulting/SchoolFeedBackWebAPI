using System.Net;
using System.Security.Claims;
using ApplicationEventWorkers.SelfOptIn;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

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
    private readonly IEvaluationRepository _evalRepo;
    private readonly IQuestionnaireRepository _questionnaireRepo;

    public TemplateSelfOptIn(
        IOptInTokenService tokens,
        IEvaluationRepository evalRepo,
        IQuestionnaireRepository questionnaireRepo)
    {
        _tokens = tokens;
        _evalRepo = evalRepo;
        _questionnaireRepo = questionnaireRepo;
    }

    private sealed class RequestDto { public string? OptInToken { get; set; } }

    [Function("TemplateSelfOptIn")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post",
         Route = "templates/{id}/self-opt-in")] HttpRequestData req,
        string id,
        FunctionContext context)   // StudentOnlyMiddleware should populate ctx.Items["User"]
    {

        var tokenCookie = req.Cookies.FirstOrDefault(c => c.Name == "token");
        if (tokenCookie == null || string.IsNullOrWhiteSpace(tokenCookie.Value))
            return req.CreateResponse(HttpStatusCode.Unauthorized);

        var token = tokenCookie.Value;
        bool isStudent = JwtRoleValidator.IsStudent(token, context);
        bool isAdmin = JwtRoleValidator.IsAdmin(token, context);
        if (!isStudent && !isAdmin)
            return req.CreateResponse(HttpStatusCode.Forbidden);

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

        // load real template by id
        var template = await _evalRepo.GetQuestionTemplateBySurveyIdAsync(id);

        if (template is null)
            return req.CreateResponse(HttpStatusCode.NotFound);

        if (!template.IsSelfOptInEnabled)
            return req.CreateResponse(HttpStatusCode.Forbidden);

        if (template.OptInExpiresAt is not null && template.OptInExpiresAt <= DateTimeOffset.UtcNow)
            return await Text(req, HttpStatusCode.Gone, "Self opt-in window has closed.");

        string templateId = $"questiontemplates_{id}";

        if (template.MaxParticipants is int max && max >= 0)
        {
            var current = await _questionnaireRepo.CountQuestionnairesForTemplateAsync(templateId);
            if (current >= max)
                return await Text(req, HttpStatusCode.Forbidden, "Capacity reached for this template.");
        }

        // idempotency check
        var alreadyExists = await _questionnaireRepo.QuestionnaireExistsForStudentAsync(id, email);
        if (alreadyExists)
        {
            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(new { status = "already_has_access" });
            return ok;
        }

        // create a Questionnaire instance for the new student
        await _questionnaireRepo.SelfOptInStudentAsync(templateGuid, email);

        var created = req.CreateResponse(HttpStatusCode.Created);
        await created.WriteAsJsonAsync(new { status = "granted" });
        return created;
    }

    private static async Task<HttpResponseData> Text(HttpRequestData req, HttpStatusCode code, string message)
    {
        var res = req.CreateResponse(code);
        await res.WriteStringAsync(message);
        return res;
    }
}
