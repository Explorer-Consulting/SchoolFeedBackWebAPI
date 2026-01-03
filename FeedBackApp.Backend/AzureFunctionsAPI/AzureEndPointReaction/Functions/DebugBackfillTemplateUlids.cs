using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using NUlid;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Core.Model;

namespace ApplicationEventWorkers.AzureEndPointReaction.Functions;

/* Admin helper function
 * This function is used to backfill the existing templates with ULID type  (templateUlid)
 * The Ulids used to be null, caused a lot of problems (discrepancy between Guid and Ulid)
 * Running the backfill as such:
 * curl -s -X POST "http://localhost:7071/api/debug/backfill-template-ulids" on mac & linux
 * or just simply access the endpoint on Windows
 * you should see something like {"scanned":3,"updated":0}
 */

public sealed class DebugBackfillTemplateUlids
{
    private readonly AppDBContext _db;
    public DebugBackfillTemplateUlids(AppDBContext db) => _db = db;

    [Function("DebugBackfillTemplateUlids")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "debug/backfill-template-ulids")]
        HttpRequestData req)
    {
        // load a page (tracked)
        var page = await _db.Set<QuestionnaireTemplate>()
            .Take(1000)
            .ToListAsync();

        // filter in memory
        var toFix = page.Where(t => string.IsNullOrWhiteSpace(t.TemplateUlid)).ToList();

        foreach (var t in toFix)
            t.TemplateUlid = Ulid.NewUlid().ToString();

        // persist
        await _db.SaveChangesAsync();

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { scanned = page.Count, updated = toFix.Count });
        return res;
    }
}