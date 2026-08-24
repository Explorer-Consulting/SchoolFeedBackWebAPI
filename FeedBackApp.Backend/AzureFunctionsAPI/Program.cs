using Application.DTOs.Questionnaire;
using Application.DTOs.Survey;
using Application.Services;
using Application.Services.Interfaces;
using Application.Validation.CreateValidation;
using ApplicationEventWorkers.SelfOptIn; 
using FeedBackApp.Backend.Infrastructure.Email;
using FeedBackApp.Core.Email;
using Azure.Core.Serialization;
using Azure.Storage.Blobs;
using AzureFunctionsAPI.AzureEndPointReaction.Functions;
using FeedBackApp.Backend.Infrastructure.Middleware;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using FeedBackApp.Backend.Infrastructure.Persistence.Context;
using FeedBackApp.Backend.Infrastructure.Persistence.Repository;
using FeedBackApp.Backend.Infrastructure.Persistence.BlobStorageInterface;
using FeedBackApp.Core.Repositories;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;
using System.Text.Json;
using Azure.Storage.Queues;

QuestPDF.Settings.License = LicenseType.Community;

// add Sentry before any particular job
SentrySdk.Init(options =>
{
    // A Sentry Data Source Name (DSN) is required.
    // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
    // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
    options.Dsn = "https://e527f85474f0587735eddc9531cae1a5@o4508133068963840.ingest.de.sentry.io/4510549831778384";

    // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
    // This might be helpful, or might interfere with the normal operation of your application.
    // We enable it here for demonstration purposes when first trying Sentry.
    // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
    options.Debug = true;

    // This option is recommended. It enables Sentry's "Release Health" feature.
    options.AutoSessionTracking = true;
});

SentrySdk.CaptureMessage("Hello Sentry");


// ──────────────────────────────────────────────────---
// 1) Modern isolated builder
// ─────────────────────────────────────────────────────
var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// ─────────────────────────────────────────────────────
// 2) JSON serializer
// ─────────────────────────────────────────────────────
builder.Services.Configure<WorkerOptions>(o =>
{
    o.Serializer = new JsonObjectSerializer(
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
});

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// ─────────────────────────────────────────────────────
// 3) EF Core Cosmos – local.settings.json / Azure App Settings
// ─────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDBContext>(options =>
{
    var endpoint = builder.Configuration["Cosmos:AccountEndpoint"];
    var key = builder.Configuration["Cosmos:AccountKey"];
    var db = builder.Configuration["Cosmos:DatabaseName"];

    if (string.IsNullOrWhiteSpace(endpoint) ||
        string.IsNullOrWhiteSpace(key) ||
        string.IsNullOrWhiteSpace(db))
    {
        throw new InvalidOperationException(
            "Missing Cosmos configuration: Cosmos:AccountEndpoint, Cosmos:AccountKey, Cosmos:DatabaseName");
    }

    options.UseCosmos(endpoint, key, db);
});

// ─────────────────────────────────────────────────────
// 4) Blob Storage – ReportStorage:ConnectionString, ReportStorage:ContainerName
// ─────────────────────────────────────────────────────
builder.Services.AddSingleton(sp =>
{
    var cs = builder.Configuration["ReportStorage:ConnectionString"];
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("Missing ReportStorage:ConnectionString");

    return new BlobServiceClient(cs);
});

builder.Services.AddSingleton<IBlobContext>(sp =>
{
    var serviceClient = sp.GetRequiredService<BlobServiceClient>();
    var containerName = builder.Configuration["ReportStorage:ContainerName"];

    if (string.IsNullOrWhiteSpace(containerName))
        throw new InvalidOperationException("Missing ReportStorage:ContainerName");

    return new BlobContext(serviceClient, containerName);
});

// ─────────────────────────────────────────────────────
// Self Opt-In
// ─────────────────────────────────────────────────────

builder.Services.AddOptions<SelfOptInJwtOptions>()
    .Configure<IConfiguration>((opt, cfg) =>
    {
        // pulled from "SelfOptInJwtOptions"
        opt.Enabled = true;
        opt.Issuer = "feedback-app.optin";          // source
        opt.Audience = "feedback-app.optin";        // destination
        opt.SigningKey = cfg["Jwt:SecretKey"]!;     // reuse existing secret
        opt.TokenTtlMinutes = 7 * 24 * 60;          // 7 days
    })
    .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey) && o.SigningKey.Length >= 32,
        "SelfOptInJwt: SigningKey must be >= 32 chars")
    .ValidateOnStart();

builder.Services.AddSingleton<IOptInTokenService, OptInJwtService>();

builder.Services.AddSingleton(sp =>
{
    var conn = builder.Configuration["AzureWebJobsStorage"]
               ?? throw new InvalidOperationException("AzureWebJobsStorage missing.");
    var client = new QueueClient(conn, "optin-email-jobs",
        new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
    client.CreateIfNotExists();
    return client;
});

// ─────────────────────────────────────────────────────
// 5) Egyéb config ellenőrzés (JWT, Google, Email, AdminEmails)
// ─────────────────────────────────────────────────────
_ = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Missing Jwt:SecretKey");

_ = builder.Configuration["Google:ClientId"]
    ?? throw new InvalidOperationException("Missing Google:ClientId");

_ = builder.Configuration["Facebook:AppId"]
    ?? throw new InvalidOperationException("Missing Facebook:AppId");

_ = builder.Configuration["Facebook:AppSecret"]
    ?? throw new InvalidOperationException("Missing Facebook:AppSecret");

_ = builder.Configuration["Microsoft:ClientId"]
    ?? throw new InvalidOperationException("Missing Microsoft:ClientId");

_ = builder.Configuration["Microsoft:TenantId"]
    ?? "common";

_ = builder.Configuration["LinkedIn:ClientId"]
    ?? throw new InvalidOperationException("Missing LinkedIn:ClientId");

_ = builder.Configuration["LinkedIn:ClientSecret"]
    ?? throw new InvalidOperationException("Missing LinkedIn:ClientSecret");

var rawAdminEmails = builder.Configuration["AdminEmails"] ?? string.Empty;
var adminEmails = rawAdminEmails
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

_ = builder.Configuration["Email:FromAddress"]
    ?? throw new InvalidOperationException("Missing Email:FromAddress");

_ = builder.Configuration["Email:FromName"]
    ?? throw new InvalidOperationException("Missing Email:FromName");

_ = builder.Configuration["Email:AppPassword"]
    ?? throw new InvalidOperationException("Missing Email:AppPassword");

// Certificates – localon használod, Azure-on majd KeyVault lesz valószínűleg
var certLoadPath = builder.Configuration["Certificates:LoadPath"];


// ─────────────────────────────────────────────────────
// 6) Services & repositories
// ─────────────────────────────────────────────────────
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();
builder.Services.AddScoped<IQuestionnaireService, QuestionnaireService>();
builder.Services.AddSingleton<Application.Services.Interfaces.IOtpService, Application.Services.OtpService>();
builder.Services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddScoped<IWhitelistRepository, WhitelistRepository>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();

builder.Services.AddScoped<IReportRepository, ReportRepository>();
// --- Email Services ---
// Email configuration: Loaded from environment variables
builder.Services.AddSingleton<FeedBackApp.Core.Email.Configuration.EmailConfiguration>(
    _ => FeedBackApp.Core.Email.Configuration.EmailConfiguration.FromEnvironment());

// Email templates: Loaded at startup and cached in memory
// Templates are loaded once and reused for all email rendering operations
builder.Services.AddSingleton<IReadOnlyDictionary<string, Application.Email.Templates.EmailTemplate>>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<Program>>();
    return Application.Email.Templates.EmailTemplateLoader.LoadTemplates(config, logger);
});

// Email template service: Renders templates with token replacement
builder.Services.AddScoped<Application.Email.Templates.IEmailTemplateService, Application.Email.Templates.EmailTemplateService>();

// Email content service: Creates email messages using templates (replaces Factory pattern)
builder.Services.AddScoped<Application.Email.IEmailContentService, Application.Email.EmailContentService>();

// Email sender: MailKit-based SMTP implementation
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<IEmailService, EmailService>();

// --- Report Services ---
// Report service uses IBlobContext for blob storage operations
builder.Services.AddScoped<EmailSendingFunctions>();
builder.Services.AddScoped<AzureFunctionsAPI.AzureEndPointReaction.Functions.AuthFunctions>();
builder.Services.AddScoped<IReportService, ReportService>();    
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// ─────────────────────────────────────────────────────
// 8) Validators
// ─────────────────────────────────────────────────────
builder.Services.AddScoped<IValidator<CreateSurveyMetadataDTO>, CreateSurveyMetadataValidator>();
builder.Services.AddScoped<IValidator<MetaTeacherDTO>, MetaTeacherValidator>();
builder.Services.AddScoped<IValidator<QuestionnaireCreationParamDTO>, QuestionnaireCreationParamValidator>();
builder.Services.AddScoped<IValidator<QuestionnaireDTO>, QuestionnaireValidator>();
builder.Services.AddScoped<IValidator<QuestionTemplateDTO>, QuestionTemplateValidator>();
builder.Services.AddScoped<IValidator<StudentSetDTO>, StudentSetValidator>();


builder.Services.AddSingleton<AdminOnlyMiddleware>();
builder.Services.AddSingleton<StudentOnlyMiddleware>();
builder.Services.AddSingleton<MiddlewareSelector>();

builder
    .UseMiddleware<MiddlewareSelector>();

// ─────────────────────────────────────────────────────
// 10) Build, DB init, Run
// ─────────────────────────────────────────────────────
var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();