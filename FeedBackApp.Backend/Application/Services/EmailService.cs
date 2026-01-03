using Application.Services.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.Repositories;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Application.Services
{
    /// <summary>
    /// Batch email orchestration and delivery service for the School Feedback application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Purpose</b><br/>
    /// Sends survey-related email notifications (student invitations, teacher/admin report deliveries),
    /// using an SMTP gateway and a repository-backed queue (<see cref="IEmailRepository"/>).
    /// It also composes future deliveries by appending items to the queue based on survey metadata.
    /// </para>
    ///
    /// <para>
    /// <b>Data flow</b><br/>
    /// <list type="number">
    ///   <item><description><c>CompileReportEmailsAsync</c> inspects survey metadata and enqueues recipients (teachers and leaders) for delivery.</description></item>
    ///   <item><description><c>SendEmailBatchAsync</c> loads the queue document, prunes expired entries, slices a daily batch, sends via SMTP, and persists the new queue state.</description></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>Security &amp; configuration</b><br/>
    /// Credentials and sender identity are supplied through environment variables:
    /// <list type="bullet">
    ///   <item><description><c>EMAIL_FROM_ADDRESS</c> – SMTP user / sender address.</description></item>
    ///   <item><description><c>EMAIL_FROM_NAME</c> – Display name for the sender.</description></item>
    ///   <item><description><c>EMAIL_APP_PASSWORD</c> – App password / secret used by the SMTP gateway.</description></item>
    ///   <item><description><c>LeaderEmail</c> – Comma-separated list of admin recipients for summary reports.</description></item>
    /// </list>
    /// Mail is sent via <c>smtp.gmail.com:587</c> with TLS (<see cref="SmtpClient.EnableSsl"/>).
    /// </para>
    ///
    /// <para>
    /// <b>Rate limiting &amp; reliability</b><br/>
    /// <c>DAILY_EMAIL_LIMIT</c> bounds the number of deliveries per batch execution. The method logs per-recipient results and
    /// updates the queue atomically after sending (removing delivered addresses, pruning empty survey entries).
    /// Attachments are created in-memory; callers should ensure report payload sizes are reasonable.
    /// </para>
    ///
    /// <para>
    /// <b>Attachments</b><br/>
    /// For teacher/admin roles, report files are downloaded via <see cref="IReportService"/> and attached using
    /// content types inferred from filename (PDF, XLSX, XLS, or binary fallback).
    /// </para>
    /// </remarks>
    public class EmailService : IEmailService
    {
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly string _appPassword;
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailRepository _emailRepository;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IReportService _reportService;

        /// <summary>
        /// Maximum number of emails the service will attempt to send in a single batch execution.
        /// </summary>
        private static short DAILY_EMAIL_LIMIT = 500;

        /// <summary>
        /// SMTP submission port used for STARTTLS on Gmail.
        /// </summary>
        private static short DNS_PORT = 587;

        /// <summary>
        /// Creates an instance of <see cref="EmailService"/> with all required dependencies and configuration.
        /// </summary>
        /// <param name="logger">Structured logger instance.</param>
        /// <param name="emailRepository">Repository that stores and retrieves the email dispatch queue document.</param>
        /// <param name="questionnaireRepository">Repository used to fetch survey metadata when composing new emails.</param>
        /// <param name="reportService">Service used to obtain report files for teacher/admin deliveries.</param>
        /// <exception cref="InvalidOperationException">Thrown if required environment variables are missing.</exception>
        public EmailService(
            ILogger<EmailService> logger,
            IEmailRepository emailRepository,
            IQuestionnaireRepository questionnaireRepository,
            IReportService reportService)
        {
        _fromAddress = Environment.GetEnvironmentVariable("Email:FromAddress") ?? throw new InvalidOperationException("EMAIL_FROM_ADDRESS is not set.");
        _fromName = Environment.GetEnvironmentVariable("Email:FromName") ?? throw new InvalidOperationException("EMAIL_FROM_NAME is not set.");
        _appPassword = Environment.GetEnvironmentVariable("Email:AppPassword") ?? throw new InvalidOperationException("EMAIL_APP_PASSWORD is not set.");
            _logger = logger;
            _emailRepository = emailRepository;
            _questionnaireRepository = questionnaireRepository;
            _reportService = reportService;
        }

        /// <summary>
        /// Sends a rate-limited batch of queued survey emails and updates the queue document accordingly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Processing steps</b><br/>
        /// Loads the email queue, removes expired student surveys, selects active items whose start date has passed,
        /// and takes up to <see cref="DAILY_EMAIL_LIMIT"/> recipient addresses across surveys. For each item, composes
        /// the subject and HTML body based on the <see cref="Role"/> and attaches reports if applicable.
        /// After successful sends, removes delivered recipients from the queue and persists the document.
        /// </para>
        /// <para>
        /// <b>Return semantics</b><br/>
        /// Returns <c>true</c> if any email was sent; <c>false</c> if the queue is empty or no eligible items were found.
        /// Any exception is caught, logged, and results in a <c>false</c> return to keep the scheduler resilient.
        /// </para>
        /// </remarks>
        /// <returns><c>true</c> if at least one message was sent; otherwise <c>false</c>.</returns>
        public async Task<bool> SendEmailBatchAsync()
        {
            try
            {
                var doc = await _emailRepository.GetEmailsDocumentAsync();
                if (doc == null || !doc.EmailsToSendList.Any())
                    return false;

                var expired = doc.EmailsToSendList
                    .Where(s => s.EndDate < DateTime.UtcNow && s.Role == FeedBackApp.Core.Model.Enum.Role.Student)
                    .ToList();

                foreach (var survey in expired)
                {
                    doc.EmailsToSendList.Remove(survey);
                    _logger.LogInformation("Removed expired survey {SurveyName} ({SurveyId})", survey.SurveyName, survey.SurveyId);
                }
            
                var activeSurveys = doc.EmailsToSendList
                    .Where(s => s.StartDate <= DateTime.UtcNow)
                    .ToList();

                if (!activeSurveys.Any())
                    return false;

                var batch = activeSurveys
                    .SelectMany(s => s.Emails.Select(e => new
                    {
                        SurveyId = s.SurveyId,
                        SurveyName = s.SurveyName,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        Email = e,
                        Role = s.Role
                    }))
                    .Take(DAILY_EMAIL_LIMIT)
                    .ToList();

                if (!batch.Any())
                    return false;

                using var smtp = new SmtpClient("smtp.gmail.com", DNS_PORT)
                {
                    Credentials = new NetworkCredential(_fromAddress, _appPassword),
                    EnableSsl = true
                };

                foreach (var entry in batch)
                {
                    var from = new MailAddress(_fromAddress, _fromName);
                    var to = new MailAddress(entry.Email);

                    string subject;
                    string body;
                    List<Attachment>? attachments = null;

                    switch (entry.Role)
                    {
                        case Role.Student:
                            subject = $"Kérdőív meghívó: {entry.SurveyName}";
                            body =
$@"Kedves diák,<br/><br/>
Kérünk töltsd ki a <b>{entry.SurveyName}</b> nevű kérdőívet, és adj visszajelzést a tanáraidnak.<br/><br/>
<a href=""https://witty-beach-0b0c08903.2.azurestaticapps.net"">Kattints ide, hogy elkezdd a kérdőívek kitöltését</a>";
                            break;

                        case Role.Teacher:
                            subject = $"Kérdőív eredmények: {entry.SurveyName}";
                            body =
$@"Kedves tanár,<br/><br/>
Csatolva megtalálja a kérdőívek összesített eredményét.<b>{entry.SurveyName}</b>.";

                            var teacherReports = await _reportService.DownloadTeacherFilesByIdPrefixAsync(entry.Email, entry.SurveyId);
                            _logger.LogInformation("Found {Count} teacher reports for {Email} / {SurveyId}",
                                                   teacherReports.Count, entry.Email, entry.SurveyId);

                            attachments = teacherReports
                                .Select(r => CreateAttachment(r.Data, r.FileName))
                                .ToList();
                            break;

                        case Role.Admin:
                            subject = $"Igazgatói összesítés: {entry.SurveyName}";
                            body =
$@"Kedves intézmény vezető,<br/><br/>
Alább csatolotuk a
<b>{entry.SurveyName}</b> kérdőív összesített eredményeit.";

                            var adminReports = await _reportService.DownloadAdminFilesByIdPrefixAsync(entry.SurveyId);
                            _logger.LogInformation("Found {Count} admin reports for {SurveyId}",
                                                   adminReports.Count, entry.SurveyId);

                            attachments = adminReports
                                .Select(r => CreateAttachment(r.Data, r.FileName))
                                .ToList();
                            break;

                        default:
                            _logger.LogWarning("Unhandled role {Role} for email {Email}", entry.Role, entry.Email);
                            continue;
                    }

                    using var message = new MailMessage(from, to)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    if (attachments != null)
                    {
                        foreach (var att in attachments)
                            message.Attachments.Add(att);
                    }

                    await smtp.SendMailAsync(message);
                    _logger.LogInformation("Sent email to {Email} for survey {SurveyName} (Role: {Role})",
                                           entry.Email, entry.SurveyName, entry.Role);
                }

                // Remove delivered recipients from the queue and drop empty survey entries.
                foreach (var e in batch)
                {
                    var surveyBatch = doc.EmailsToSendList.FirstOrDefault(s => s.SurveyId == e.SurveyId);
                    if (surveyBatch != null)
                        surveyBatch.Emails.Remove(e.Email);

                    if (surveyBatch != null && !surveyBatch.Emails.Any())
                    {
                        doc.EmailsToSendList.Remove(surveyBatch);
                    }
                }

                await _emailRepository.UpdateEmailsDocumentAsync(doc);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending batch of emails");
                return false;
            }
        }

        /// <summary>
        /// Composes and enqueues teacher and admin report deliveries for a given survey.
        /// </summary>
        /// <remarks>
        /// Fetches survey metadata to determine teacher recipients and time window, then appends entries
        /// for teachers and leaders to the email dispatch queue document. The actual delivery is performed
        /// later by <see cref="SendEmailBatchAsync"/>.
        /// </remarks>
        /// <param name="surveyId">Identifier of the target survey whose reports should be delivered.</param>
        public async Task CompileReportEmailsAsync(Guid surveyId)
        {
            var metadata = await _questionnaireRepository.GetSurveyMetadataAsync(surveyId);
            if (metadata != null)
            {
                var teachers = metadata.Teachers
                    .Where(t => !string.IsNullOrWhiteSpace(t.Email))
                    .Select(t => t.Email)
                    .ToList();

                if (!teachers.Any())
                    return;

                var emailDocument = await _emailRepository.GetEmailsDocumentAsync();
                if (emailDocument == null)
                {
                    emailDocument = new EmailsToSend
                    {
                        EmailsToSendList = new List<Email>()
                    };
                }

                var teacherEmailsToSend = new Email()
                {
                    Emails = teachers,
                    StartDate = metadata.StartDate,
                    EndDate = metadata.EndDate,
                    Role = Role.Teacher,
                    SurveyId = surveyId.ToString(),
                    SurveyName = metadata.Title
                };
                emailDocument.EmailsToSendList.Add(teacherEmailsToSend);

                var leaders = Environment.GetEnvironmentVariable("LeaderEmail") ?? "";
                var leadersEmails = leaders.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var leaderEmailsToSend = new Email()
                {
                    Emails = leadersEmails,
                    StartDate = metadata.StartDate,
                    EndDate = metadata.EndDate,
                    Role = Role.Admin,
                    SurveyId = surveyId.ToString(),
                    SurveyName = metadata.Title
                };
                emailDocument.EmailsToSendList.Add(leaderEmailsToSend);

                await _emailRepository.UpdateEmailsDocumentAsync(emailDocument);
            }
        }

        /// <summary>
        /// Creates a MIME attachment from raw bytes and a filename, inferring the content type from file extension.
        /// </summary>
        /// <param name="data">Raw file bytes.</param>
        /// <param name="fileName">File name including extension (used to infer content type).</param>
        /// <returns>An in-memory <see cref="Attachment"/> ready to be appended to <see cref="MailMessage"/>.</returns>
        private static Attachment CreateAttachment(byte[] data, string fileName)
        {
            string contentType = fileName.ToLowerInvariant() switch
            {
                string f when f.EndsWith(".pdf") => "application/pdf",
                string f when f.EndsWith(".xlsx") => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                string f when f.EndsWith(".xls") => "application/vnd.ms-excel",
                _ => "application/octet-stream"
            };

            var stream = new MemoryStream(data);
            stream.Position = 0; // ensure start
            return new Attachment(stream, fileName, contentType);
        }
    }
}
