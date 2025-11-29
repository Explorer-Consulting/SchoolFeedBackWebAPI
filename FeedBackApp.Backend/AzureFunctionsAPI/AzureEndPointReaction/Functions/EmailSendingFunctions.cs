using Application.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions;

/// <summary>
/// Azure Function for processing and sending email batches on a scheduled basis.
/// Runs daily at midnight (00:00:00) to send pending emails.
/// </summary>
public sealed class EmailSendingFunctions
{
    private readonly ILogger<EmailSendingFunctions> _logger;
    private readonly IEmailService _emailService;

    public EmailSendingFunctions(
        ILogger<EmailSendingFunctions> logger, 
        IEmailService emailService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    /// <summary>
    /// Timer-triggered function that processes and sends pending email batches.
    /// Scheduled to run daily at midnight (CRON: "0 0 0 * * *").
    /// </summary>
    /// <param name="myTimer">Timer information including schedule status.</param>
    [Function("EmailSendingFunctions")]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
    {
        var executionTime = DateTime.UtcNow;
        _logger.LogInformation(
            "Email batch processing function triggered at: {ExecutionTime} (UTC)", 
            executionTime);

        if (myTimer.ScheduleStatus is not null) 
        {
            _logger.LogInformation(
                "Next scheduled execution: {NextSchedule} (UTC)", 
                myTimer.ScheduleStatus.Next);
        }

        try
        {
            _logger.LogInformation("Starting email batch processing...");
            
            var result = await _emailService.SendEmailBatchAsync();

            if (result)
            {
                _logger.LogInformation(
                    "Email batch processing completed successfully at {ExecutionTime} (UTC)", 
                    executionTime);
            }
            else
            {
                _logger.LogInformation(
                    "Email batch processing completed with no emails to send at {ExecutionTime} (UTC)", 
                    executionTime);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "An error occurred while processing email batch at {ExecutionTime} (UTC). Error: {ErrorMessage}", 
                executionTime, 
                ex.Message);
            
            // Re-throw to ensure Azure Functions marks the execution as failed
            throw;
            /*
             Do not throw any exception inside an azure function. We usually use self-written wrapper classes like Result<T> or more frequently we use a packege for that like FluentResults. 
             From this wrapper we can build the proper response body with the status code and futher information if needed.
             Never use raw Ecxeption!.
             Do not bother yourself with this for the time being, but keep in mind for the future.
             
             */
        }
    }

}