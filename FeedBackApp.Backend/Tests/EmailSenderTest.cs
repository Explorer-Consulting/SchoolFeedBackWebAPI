using FeedBackApp.Core.Email;
using FeedBackApp.Core.Email.Configuration;
using FeedBackApp.Core.Email.Models;
using FeedBackApp.Backend.Infrastructure.Email;
using FluentAssertions;
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

    [SetUp]
    public void Setup()
    {
        // Load email configuration from environment variables
        // Ensure these are set: EMAIL_FROM_ADDRESS, EMAIL_FROM_NAME, EMAIL_APP_PASSWORD
        try
        {
            _emailConfig = EmailConfiguration.FromEnvironment();
        }
        catch (InvalidOperationException ex)
        {
            Assert.Fail($"Email configuration not set up correctly: {ex.Message}. " +
                       "Please set EMAIL_FROM_ADDRESS, EMAIL_FROM_NAME, and EMAIL_APP_PASSWORD environment variables.");
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
        // You can set a test email address via environment variable or modify this value
        // Example: var testEmailAddress = Environment.GetEnvironmentVariable("TEST_EMAIL_ADDRESS") ?? "your-test-email@example.com";
        var testEmailAddress = Environment.GetEnvironmentVariable("TEST_EMAIL_ADDRESS") 
            ?? throw new InvalidOperationException(
                "TEST_EMAIL_ADDRESS environment variable not set. " +
                "Set this to any email address you want to use for testing.");

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

