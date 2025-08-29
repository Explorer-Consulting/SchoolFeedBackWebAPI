
using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Persistence.Repository;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace Application.Services
{
    // A thread-safe, CRON-based timer service that can manually start and stop.
    // It triggers a target function (SendEmails) according to the CRON schedule.
    public class CronTimerService : ICronTimerService
    {
        private readonly ILogger<CronTimerService> _logger;
        private readonly IServiceProvider _provider;
        private readonly CrontabSchedule _schedule;
        private Timer? _timer;
        private bool _running;
        private int _isExecuting;

        public CronTimerService(ILogger<CronTimerService> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;

            // hardcoded every minute (put it away from here)
            _schedule = CrontabSchedule.Parse("* * * * *");
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public void Start()
        {
            if (_running) return;

            _logger.LogInformation("CronTimerService started.");
            _running = true;
            // Schedule the next tick based on CRON
            ScheduleNextTick();
        }

        public void Stop()
        {
            if (!_running) return;

            _logger.LogInformation("CronTimerService stopped.");
            // Stop the internal timer
            _timer?.Change(Timeout.Infinite, 0);
            _running = false;
        }

        // Schedules the next timer tick according to the CRON schedule.
        private void ScheduleNextTick()
        {
            if(!_running) return;

            // Calculate the next occurrence based on current time
            var next = _schedule.GetNextOccurrence(DateTime.Now);
            var delay = next - DateTime.Now;

            _logger.LogInformation($"Next tick scheduled at {next}");

            // Schedule a single .NET timer tick at the computed delay
            _timer = new Timer(async _ =>
            {
                await ExecuteSafeAsync(); // Execute the target function safely
                if (_running)
                    ScheduleNextTick(); // Recursively schedule the next tick
            }, null, delay, Timeout.InfiniteTimeSpan); // Only trigger once; reschedule manually
        }

        private async Task ExecuteSafeAsync()
        {
            if (Interlocked.Exchange(ref _isExecuting, 1) == 1)
            {
                _logger.LogWarning("Previous execution still running. Skipping this tick.");
                return;
            }

            try
            {
                _logger.LogInformation("Executing emial sender function...");
                using var scope = _provider.CreateScope(); //  create a new scope every tick
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var emailRepo = scope.ServiceProvider.GetRequiredService<IEmailRepository>();

                var emails = await emailRepo.GetEmailsToSend();
                if (!emails.Any()) { Stop(); return; }

                var successfullySent = new List<string>();

                // Send each email individually and track success
                foreach (var email in emails)
                {
                    bool sent = await emailService.SendEmailAsync(
                        email,
                        "Student-teacher feedback",
                        "Please complete the following questionnaires and give constructive feedback to your teachers! https://witty-beach-0b0c08903.2.azurestaticapps.net"
                    );

                    if (sent) successfullySent.Add(email);
                }

                // Remove successfully sent emails from Cosmos DB
                if (successfullySent.Any())
                {
                    await emailRepo.RemoveEmailsAsync(successfullySent);
                    _logger.LogInformation("Removed {Count} successfully sent emails from Cosmos DB", successfullySent.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function email sender");
            }
            finally
            {
                Interlocked.Exchange(ref _isExecuting, 0); // Reset execution flag
            }
        }

    }
}

