using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using ApplicationEventWorkers.SelfOptIn;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace ApplicationEventWorkers.AzureEndPointReaction.Functions;

/// <summary>
/// Bulk-send opt-in links for a QuestionnaireTemplate (GUID) to a whitelist in DB.
/// - Uses property-based Queue output binding
/// - Applies explicit Cosmos discriminator filters to avoid materialization errors.
/// </summary>
public sealed class AdminBulkSendFromDb
{
    private readonly AppDBContext _db;
    private readonly IOptInTokenService _tokens;
    private readonly IWhitelistRepository _whitelistRepository;

    public AdminBulkSendFromDb(AppDBContext db, IOptInTokenService tokens, IWhitelistRepository whitelistRepository)
    {
        _db = db;
        _tokens = tokens;
        _whitelistRepository = whitelistRepository;
    }

    [Function("AdminBulkSendFromWhitelist")]
    public async Task<Output> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post",
            Route = "ops/optin/bulk-send-from-db")] HttpRequestData req)
    {
        // 1) Parse & validate request
        var input = await req.ReadFromJsonAsync<BulkSendRequest>() ?? new();

        if (!Guid.TryParse(input.TemplateId, out var templateGuid))
            return await Fail(req, HttpStatusCode.BadRequest, "Invalid TemplateId (GUID expected).");

        if (string.IsNullOrWhiteSpace(input.BaseUrl))
            return await Fail(req, HttpStatusCode.BadRequest, "BaseUrl is required.");

        var baseUrl = input.BaseUrl.TrimEnd('/');
        if (!Regex.IsMatch(baseUrl, @"^https?://", RegexOptions.IgnoreCase))
            return await Fail(req, HttpStatusCode.BadRequest, "BaseUrl must start with http:// or https://.");

        if (input.ExpiresMinutes <= 0 || input.ExpiresMinutes > 60 * 24 * 14) // up to 14 days
            return await Fail(req, HttpStatusCode.BadRequest, "ExpiresMinutes out of range (1..20160).");

        // 2) Load template by storage Id and guard by discriminator
        var storageId = $"questiontemplates_{templateGuid:D}";
        var template = await _db.Set<QuestionnaireTemplate>()
            .AsNoTracking()
            .Where(t => EF.Property<string>(t, "DocumentType") == "QuestionTemplate")
            .SingleOrDefaultAsync(t => t.Id == storageId);

        if (template is null)
            return await Fail(req, HttpStatusCode.NotFound, $"Template not found: '{storageId}'.");

        if (!template.IsSelfOptInEnabled)
            return await Fail(req, HttpStatusCode.Forbidden, "Self opt-in disabled for this template.");

        if (template.OptInExpiresAt is not null && template.OptInExpiresAt <= DateTimeOffset.UtcNow)
            return await Fail(req, HttpStatusCode.Gone, "Self opt-in window has closed.");

        // 3) Load whitelist (DB doc id must match; case-sensitive)
        var wlId = string.IsNullOrWhiteSpace(input.WhitelistId) ? "StudentWhitelist" : input.WhitelistId!;
        var wl = await _db.Set<StudentWhitelist>()
            .AsNoTracking()
            .Where(x => EF.Property<string>(x, "DocumentType") == "StudentWhitelist")
            .SingleOrDefaultAsync(x => x.Id == wlId);

        if (wl is null)
            return await Fail(req, HttpStatusCode.NotFound, $"Whitelist '{wlId}' not found.");

        // var recipients = (wl.StudentEmails ?? new List<string>())
        //     .Select(e => (e ?? string.Empty).Trim())
        //     .Where(e => e.Length > 0)
        //     .Distinct(StringComparer.OrdinalIgnoreCase)
        //     .ToList();
        var studentEmails = await _whitelistRepository.GetStudentWhitelistAsync();
        var recipients = studentEmails?.StudentEmails ?? new List<String>();

        if (recipients.Count == 0)
            return await Fail(req, HttpStatusCode.BadRequest, "Whitelist contains no emails.");

        // 4) Build tokens, links, and messages
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(input.ExpiresMinutes);

        var subject = string.IsNullOrWhiteSpace(input.Subject)
            ? $"Feedback access – {template.Title}"
            : input.Subject!;
        var campaign = string.IsNullOrWhiteSpace(input.CampaignId)
            ? $"optin-{templateGuid:D}"
            : input.CampaignId!;

        var messages = new List<string>(input.DryRun ? 0 : recipients.Count);
        var sampleLinks = new List<string>(Math.Min(5, recipients.Count));

        foreach (var email in recipients)
        {
            var tag = input.Personalized
                ? $"@{templateGuid:D}:{email.ToLowerInvariant()}"
                : $"@{templateGuid:D}";

            var token = _tokens.CreateToken(templateGuid, tag, expiresAt);
            var link = $"{baseUrl}/api/templates/{templateGuid:D}/preview?optin={UrlEncoder.Default.Encode(token)}";

            if (input.DryRun && sampleLinks.Count < 5)
                sampleLinks.Add(link);

            if (input.DryRun) continue;

            var html = BuildHtml(template.Title ?? string.Empty, link, expiresAt);

            var payload = JsonSerializer.Serialize(new
            {
                recipientEmail = email,
                templateId = templateGuid.ToString("D"),
                optInToken = token,
                previewLink = link,
                subject,
                html,
                campaignId = campaign
            });

            messages.Add(payload);
        }

        // 5) Build HTTP response and return composite output
        var res = req.CreateResponse(input.DryRun ? HttpStatusCode.OK : HttpStatusCode.Accepted);
        await res.WriteAsJsonAsync(new
        {
            status = input.DryRun ? "dry_run" : "enqueued",
            templateId = templateGuid.ToString("D"),
            whitelist = wlId,
            recipients = recipients.Count,
            enqueued = input.DryRun ? 0 : messages.Count,
            // sampleLinks = input.DryRun ? sampleLinks : Array.Empty<string>()
        });

        return new Output
        {
            HttpResponse = res,
            QueueMessages = input.DryRun ? Array.Empty<string>() : messages.ToArray()
        };
    }

    // ---------- Helpers ----------

    private static async Task<Output> Fail(HttpRequestData req, HttpStatusCode code, string message)
    {
        var res = req.CreateResponse(code);
        await res.WriteStringAsync(message);
        return new Output { HttpResponse = res, QueueMessages = Array.Empty<string>() };
    }

    private static string BuildHtml(string title, string link, DateTimeOffset expiresAt)
    {
        var sb = new StringBuilder();
        sb.Append("<div style='font-family:Arial,Helvetica,sans-serif;font-size:14px;'>");
        sb.Append($"<p>You have been invited to complete the questionnaire: <b>{WebUtility.HtmlEncode(title)}</b>.</p>");
        sb.Append("<p>Click the button below to preview and opt in:</p>");
        sb.Append($"<p><a href=\"{link}\" style='padding:10px 16px;background:#2563EB;color:#fff;text-decoration:none;border-radius:6px;'>Open questionnaire</a></p>");
        sb.Append($"<p style='color:#555'>This link expires at <b>{expiresAt:u}</b>.</p>");
        sb.Append("<p>If you did not expect this email, you can safely ignore it.</p>");
        sb.Append("</div>");
        return sb.ToString();
    }

    // ---------- DTOs / Output ----------

    private sealed class BulkSendRequest
    {
        public string TemplateId { get; set; } = string.Empty;  // GUID string
        public int ExpiresMinutes { get; set; } = 60;
        public string BaseUrl { get; set; } = "http://localhost:7071";
        public bool Personalized { get; set; } = true;
        public string WhitelistId { get; set; } = "StudentWhitelist";
        public bool DryRun { get; set; } = true;
        public string? CampaignId { get; set; }
        public string? Subject { get; set; }
    }

    public sealed class Output
    {
        public HttpResponseData HttpResponse { get; set; } = default!;

        // Property-based Queue output binding (isolated worker)
        [QueueOutput("optin-email-jobs")]
        public string[] QueueMessages { get; set; } = Array.Empty<string>();
    }
}
