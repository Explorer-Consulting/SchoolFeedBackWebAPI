using Application.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions;

public sealed class EmailSendingFunctions(ILogger<EmailSendingFunctions> logger, IEmailService emailService)
{
    private readonly ILogger _logger = logger;
    private readonly IEmailService _emailService = emailService;

    [Function("EmailSendingFunctions")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }

        try
        {
            await _emailService.SendEmailBatchAsync();

            _logger.LogInformation("Email processing finished successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing pending emails.");
        }
    }
}