using Application.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsAPI.AzureEndPointReaction.Functions
{
    /// <summary>
    /// Timer-triggered background job that orchestrates batch email sending for the application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Purpose</b><br/>
    /// Periodically invokes the email service to process and send pending messages in bulk. The function is designed
    /// to run unattended on a fixed schedule and to emit structured logs for observability and operations.
    /// </para>
    ///
    /// <para>
    /// <b>Trigger &amp; Schedule</b><br/>
    /// The function is activated by an Azure Functions <see cref="TimerTriggerAttribute"/> on a CRON expression.
    /// The CRON used here (<c>0 0 0 * * *</c>) executes the job once per day at 00:00:00 (UTC unless an app time zone
    /// is configured via <c>WEBSITE_TIME_ZONE</c> or <c>functions:timezone</c>). The <see cref="TimerInfo"/> argument
    /// provides metadata about past and next schedule occurrences through <see cref="TimerScheduleStatus"/>.
    /// </para>
    ///
    /// <para>
    /// <b>Execution semantics</b><br/>
    /// The function logs the execution timestamp and, when available, the next scheduled time. It then delegates the
    /// batch processing to <see cref="IEmailService"/>. Any exception thrown during processing is caught and recorded
    /// as an error log entry to avoid crashing the host. This pattern favors reliability and continuous operation in
    /// scheduled, long-running environments.
    /// </para>
    ///
    /// <para>
    /// <b>Idempotency &amp; safety</b><br/>
    /// The function itself is stateless and idempotency is expected to be enforced by <see cref="IEmailService"/>.
    /// The service should ensure that messages are not sent multiple times and that partial failures can be retried
    /// safely (e.g., via durable status flags, transactional outbox, or at-least-once delivery strategies).
    /// </para>
    ///
    /// <para>
    /// <b>Observability</b><br/>
    /// Logs are emitted for start, schedule hints, successful completion, and failure. For production, it is recommended
    /// to integrate with a telemetry system (e.g., Azure Application Insights) and to include metrics that reflect
    /// batch size, success/failure counts, and durations to support SLOs and alerting.
    /// </para>
    ///
    /// <para>
    /// <b>Configuration</b><br/>
    /// The concrete behavior of <see cref="IEmailService"/> (e.g., SMTP provider, API credentials, throttling limits)
    /// is expected to be configured via environment variables or app settings and injected through DI at startup.
    /// </para>
    /// </remarks>
    /// <param name="logger">Structured logger for operational diagnostics.</param>
    /// <param name="emailService">Application email service responsible for batching and sending messages.</param>
    public sealed class EmailSendingFunctions(ILogger<EmailSendingFunctions> logger, IEmailService emailService)
    {
        private readonly ILogger _logger = logger;
        private readonly IEmailService _emailService = emailService;

        /// <summary>
        /// Executes the daily batch email sending workflow on a timer schedule.
        /// </summary>
        /// <remarks>
        /// The function writes informational logs for the execution timestamp and the next scheduled run (if available),
        /// then invokes <see cref="_emailService"/> to process pending emails. Any exceptions are caught, logged as errors,
        /// and the function returns control to the host to continue future scheduled executions.
        /// </remarks>
        /// <param name="myTimer">
        /// Timer metadata including last, next, and current schedule information as supplied by the Azure Functions host.
        /// </param>
        [Function("EmailSendingFunctions")]
        public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
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
}
