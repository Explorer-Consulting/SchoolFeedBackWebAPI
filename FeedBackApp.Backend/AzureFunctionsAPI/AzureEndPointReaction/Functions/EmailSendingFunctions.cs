using Application.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions;

public sealed class EmailSendingFunctions(ILogger<EmailSendingFunctions> logger, IEmailService emailService)
{

    [Function(nameof(EmailSendingFunctions))]
    public async Task RunAsync([TimerTrigger("%Email:BatchSchedule%")] TimerInfo timer)
    {
        logger.LogInformation("EmailSendingTimer fired at {ExecutionTime}. IsPastDue = {IsPastDue}", DateTimeOffset.UtcNow, timer.IsPastDue);

        if (timer.ScheduleStatus is not null)
        {
            logger.LogInformation("Last: {Last}, Next: {Next}, LastUpdated: {LastUpdated}",
                timer.ScheduleStatus.Last,
                timer.ScheduleStatus.Next,
                timer.ScheduleStatus.LastUpdated);
        }

        try
        {
            await emailService.SendEmailBatchAsync();

            logger.LogInformation("Email processing finished successfully.");
        }
        catch (Exception ex) // some custom exception would be fain.
        {
            logger.LogError(ex, "Error while processing pending emails.");
            throw;
        }
    }
}