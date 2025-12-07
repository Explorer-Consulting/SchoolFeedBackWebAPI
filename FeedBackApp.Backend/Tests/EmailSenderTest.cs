using FeedBackApp.Core.Email;
using FeedBackApp.Core.Email.Configuration;
using FeedBackApp.Core.Email.Models;
using FeedBackApp.Backend.Infrastructure.Email;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Tests;

/// <summary>
/// Test class for email sending functionality.
/// This test can be used to verify that the email sending system is working correctly.
/// 
/// Note: You can configure any email address for testing by setting the TEST_EMAIL_ADDRESS
/// environment variable, or by modifying the testEmailAddress variable in the test method.
/// </summary>
[TestFixture]
public class EmailSenderTest
{
    // Note: These fields are intentionally nullable as they require manual setup for integration testing
    // The test is marked with [Ignore] and requires actual email configuration to run
    private IEmailSender? _emailSender;
    private EmailConfiguration? _emailConfig;
    private IConfiguration? _configuration;

    [SetUp]
    public void Setup()
    {
        // Load email configuration from AzureFunctionsAPI/local.settings.json (for testing only)
        try
        {
            // Get the solution root directory (go up from Tests/bin/Debug/net9.0 to Tests, then to solution root)
            var testAssemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) 
                ?? Directory.GetCurrentDirectory();
            
            // Navigate from bin/Debug/net9.0 -> Tests -> solution root -> AzureFunctionsAPI
            var solutionRoot = Path.GetFullPath(Path.Combine(testAssemblyPath, "..", "..", "..", ".."));
            var azureFunctionsPath = Path.Combine(solutionRoot, "AzureFunctionsAPI");
            var settingsPath = Path.Combine(azureFunctionsPath, "local.settings.json");
            
            if (!File.Exists(settingsPath))
            {
                throw new FileNotFoundException($"AzureFunctionsAPI/local.settings.json not found at: {settingsPath}");
            }
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(azureFunctionsPath)
                .AddJsonFile("local.settings.json", optional: false, reloadOnChange: false)
                .Build();

            // Azure Functions local.settings.json has values under "Values" section
            _emailConfig = new EmailConfiguration
            {
                FromAddress = _configuration["Values:Email:FromAddress"] 
                    ?? throw new InvalidOperationException("Values:Email:FromAddress is not set in AzureFunctionsAPI/local.settings.json"),
                FromName = _configuration["Values:Email:FromName"] 
                    ?? throw new InvalidOperationException("Values:Email:FromName is not set in AzureFunctionsAPI/local.settings.json"),
                AppPassword = _configuration["Values:Email:AppPassword"] 
                    ?? throw new InvalidOperationException("Values:Email:AppPassword is not set in AzureFunctionsAPI/local.settings.json"),
                LeaderEmails = _configuration["Values:AdminEmails"] ?? string.Empty
            };
        }
        catch (InvalidOperationException ex)
        {
            Assert.Fail($"Email configuration not set up correctly: {ex.Message}. " +
                       "Please ensure AzureFunctionsAPI/local.settings.json exists with Values:Email:FromAddress, Values:Email:FromName, and Values:Email:AppPassword.");
        }
        catch (FileNotFoundException)
        {
            Assert.Fail("AzureFunctionsAPI/local.settings.json file not found. Please ensure it exists.");
        }

        // Initialize the email sender for integration testing
        if (_emailConfig != null)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<SmtpEmailSender>();
            _emailSender = new SmtpEmailSender(_emailConfig, logger);
        }
    }

    [Test]
    // [Ignore("Integration test - requires email configuration and actual SMTP connection. " +
    //         "Remove [Ignore] attribute and set TEST_EMAIL_ADDRESS environment variable to run this test.")]
    public async Task SendTestEmail_WithValidConfiguration_ShouldSendSuccessfully()
    {
        // Arrange
        // Load test email address from local.settings.json
        if (_configuration == null)
        {
            Assert.Fail("Configuration not initialized. Check Setup method.");
        }

        // Use first email from AdminEmails as test email, or allow override via environment variable
        var testEmailAddress = Environment.GetEnvironmentVariable("TEST_EMAIL_ADDRESS")
            ?? _configuration!["Values:TestEmailAddress"]
            ?? (_configuration!["Values:AdminEmails"]?.Split(',')[0]?.Trim())
            ?? throw new InvalidOperationException(
                "TestEmailAddress is not set. Please set TEST_EMAIL_ADDRESS environment variable, " +
                "add Values:TestEmailAddress to AzureFunctionsAPI/local.settings.json, " +
                "or ensure Values:AdminEmails contains at least one email address.");

        if (_emailSender == null || _emailConfig == null)
        {
            Assert.Fail("Email sender or configuration not initialized. Check Setup method.");
        }

        var testMessage = new EmailMessage
        {
            To = testEmailAddress,
            Subject = "Test Email - FeedBackApp Email System",
            Body = @"<html>
<body>
    <h2>Test Email from FeedBackApp</h2>
    <p>This is a test email to verify that the email sending system is working correctly.</p>
    <p><strong>Timestamp:</strong> " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + @"</p>
    <p>If you received this email, the MailKit-based email sender is functioning properly.</p>
    <hr>
    <p><em>This is an automated test email from the FeedBackApp backend system.</em></p>
</body>
</html>",
            IsHtml = true,
            Attachments = new List<EmailAttachment>()
        };

        // Act
        var result = await _emailSender!.SendEmailAsync(testMessage);

        // Assert
        result.Should().BeTrue("Email should be sent successfully when configuration is valid.");
    }

    [Test]
    // [Ignore("Integration test - requires email configuration and actual SMTP connection.")]
    public async Task SendTestEmail_ToAllAdminEmails_ShouldSendToAllRecipients()
    {
        // Arrange
        if (_configuration == null || _emailSender == null)
        {
            Assert.Fail("Configuration or email sender not initialized. Check Setup method.");
        }

        // Get all admin emails from configuration
        var adminEmailsString = _configuration["Values:AdminEmails"];
        if (string.IsNullOrWhiteSpace(adminEmailsString))
        {
            Assert.Fail("AdminEmails is not set in AzureFunctionsAPI/local.settings.json");
        }

        var adminEmails = adminEmailsString
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .ToList();

        if (!adminEmails.Any())
        {
            Assert.Fail("No admin emails found in AdminEmails configuration");
        }

        Console.WriteLine($"Sending test emails to {adminEmails.Count} admin email(s): {string.Join(", ", adminEmails)}");

        // Send email to each admin email address
        var emailTasks = adminEmails.Select(async email =>
        {
            var testMessage = new EmailMessage
            {
                To = email,
                Subject = $"Test Email - FeedBackApp Email System ({email})",
                Body = $@"<html>
<body>
    <h2>Test Email from FeedBackApp</h2>
    <p>This is a test email sent to: <strong>{email}</strong></p>
    <p>This email is being sent to verify that the email sending system works for all admin email addresses.</p>
    <p><strong>Timestamp:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}</p>
    <p>If you received this email, the MailKit-based email sender is functioning properly for this address.</p>
    <hr>
    <p><em>This is an automated test email from the FeedBackApp backend system.</em></p>
</body>
</html>",
                IsHtml = true,
                Attachments = new List<EmailAttachment>()
            };

            var success = await _emailSender!.SendEmailAsync(testMessage);
            return (Email: email, Success: success);
        });

        // Wait for all emails to be sent in parallel
        var results = await Task.WhenAll(emailTasks);

        // Assert
        var successful = results.Where(r => r.Success).ToList();
        var failed = results.Where(r => !r.Success).ToList();

        Console.WriteLine($"Email sending results:");
        Console.WriteLine($"  Successful ({successful.Count}): {string.Join(", ", successful.Select(r => r.Email))}");
        if (failed.Any())
        {
            Console.WriteLine($"  Failed ({failed.Count}): {string.Join(", ", failed.Select(r => r.Email))}");
        }

        successful.Should().HaveCount(adminEmails.Count, 
            $"All {adminEmails.Count} admin emails should be sent successfully. " +
            $"Failed: {string.Join(", ", failed.Select(r => r.Email))}");
    }

    [Test]
    public void EmailConfiguration_FromEnvironment_ShouldLoadRequiredSettings()
    {
        // Arrange & Act
        var config = EmailConfiguration.FromEnvironment();

        // Assert
        config.Should().NotBeNull();
        config.FromAddress.Should().NotBeNullOrWhiteSpace("EMAIL_FROM_ADDRESS should be set.");
        config.FromName.Should().NotBeNullOrWhiteSpace("EMAIL_FROM_NAME should be set.");
        config.AppPassword.Should().NotBeNullOrWhiteSpace("EMAIL_APP_PASSWORD should be set.");
        config.SmtpHost.Should().NotBeNullOrWhiteSpace("SMTP host should have a default value.");
        config.SmtpPort.Should().BeGreaterThan(0, "SMTP port should be a valid port number.");
    }

    [Test]
    public void EmailMessage_WithValidData_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "<p>Test Body</p>",
            IsHtml = true,
            Attachments = new List<EmailAttachment>
            {
                new EmailAttachment
                {
                    Data = new byte[] { 1, 2, 3 },
                    FileName = "test.pdf",
                    ContentType = "application/pdf"
                }
            }
        };

        // Assert
        message.To.Should().Be("test@example.com");
        message.Subject.Should().Be("Test Subject");
        message.Body.Should().Contain("Test Body");
        message.IsHtml.Should().BeTrue();
        message.Attachments.Should().HaveCount(1);
        message.Attachments[0].FileName.Should().Be("test.pdf");
        message.Attachments[0].ContentType.Should().Be("application/pdf");
    }
}

