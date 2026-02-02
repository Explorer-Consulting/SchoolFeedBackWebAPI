using Application.Services.Interfaces;
using ApplicationEventWorkers.SelfOptIn;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;

namespace ApplicationEventWorkers.AzureEndPointReaction.Functions;

public sealed class SendOptInEmailWorker
{
    private readonly IOptInTokenService _tokens;
    private readonly IEmailService _email;
    private readonly IEmailRenderer _renderer;
    private readonly AppDBContext _db;

    public SendOptInEmailWorker(IOptInTokenService tokens, IEmailService email, IEmailRenderer renderer, AppDBContext db)
    { _tokens = tokens; _email = email; _renderer = renderer; _db = db; }

    [Function("SendOptInEmailWorker")]
    public async Task Run([QueueTrigger("optin-email-jobs")] OptInEmailJob job)
    {
        var item = await _db.Set<EmailCampaignItem>().FirstOrDefaultAsync(x =>
            x.CampaignId == job.CampaignId && x.RecipientEmail == job.RecipientEmail);

        if (item is null) return; // campaign cleaned up

        try
        {
            item.Attempt++;

            var token = job.Personalized
                ? _tokens.CreateTokenWithEmail(job.TemplateGuid, tag: "", job.RecipientEmail, job.ExpiresAtUtc)
                : _tokens.CreateToken(job.TemplateGuid, tag: "", job.ExpiresAtUtc);

            var link = $"{job.BaseUrl.TrimEnd('/')}/api/templates/{job.TemplateGuid:D}/preview?optin={Uri.EscapeDataString(token)}";
            var (subject, html) = _renderer.BuildOptInMail(job.Subject, job.RecipientName ?? "", link, job.ExpiresAtUtc, templateTitle: "");
            
            _db.Add(new OutboundEmail {
                To = job.RecipientEmail,
                Subject = subject,
                HtmlBody = html,
                Type = "SelfOptIn",
                CampaignId = job.CampaignId 
            });
            await _db.SaveChangesAsync();
            await _email.SendEmailBatchAsync();
            
            item.Status = "sent";
            item.SentAt = DateTimeOffset.UtcNow;

            var campaign = await _db.Set<EmailCampaign>().FirstAsync(c => c.Id == job.CampaignId);
            campaign.Sent++; campaign.Queued = Math.Max(0, campaign.Queued - 1);
        }
        catch (Exception ex)
        {
            item.Status = "failed";
            item.Error = ex.Message;
            var campaign = await _db.Set<EmailCampaign>().FirstAsync(c => c.Id == job.CampaignId);
            campaign.Failed++; campaign.Queued = Math.Max(0, campaign.Queued - 1);
        }
        await _db.SaveChangesAsync();
    }
}
