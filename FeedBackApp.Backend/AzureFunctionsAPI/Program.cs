using Application.DTOs.Questionnaire;
using Application.DTOs.Survey;
using Application.Email.Builders;
using Application.Services;
using Application.Services.Interfaces;
using Application.Validation.CreateValidation;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;
using System.Text.Json;
using Azure.Identity;

QuestPDF.Settings.License = LicenseType.Community;

var host = new HostBuilder()
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true)
           .AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();

        services.Configure<WorkerOptions>(o =>
        {
            o.Serializer = new JsonObjectSerializer(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        });

        // --- EF Core (Cosmos) ---
        services.AddDbContext<AppDBContext>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString")
                ?? throw new InvalidOperationException("ConnectionString environment variable is not set.");

            options.UseCosmos(connectionString, databaseName: "SchoolDatabase");
        });

        // --- Blob Storage DI (ÚJ: BlobServiceClient + IBlobContext ---
        services.AddSingleton(sp =>
        {
            var cs = Environment.GetEnvironmentVariable("AZURE_REPORT_BLOB_STORAGE");
            if (!string.IsNullOrWhiteSpace(cs))
                return new BlobServiceClient(cs);

            throw new InvalidOperationException("Set AZURE_REPORT_BLOB_STORAGE.");
        });

        services.AddSingleton<IBlobContext>(sp =>
        {
            var svc = sp.GetRequiredService<BlobServiceClient>();
            var containerName = Environment.GetEnvironmentVariable("AZURE_REPORTS_CONTAINER")
                ?? throw new InvalidOperationException("AZURE_REPORTS_CONTAINER is not set.");
            return new BlobContext(svc, containerName); // CreateIfNotExists itt lefut a konstruktorban
        });

        // --- Application Services ---
        services.AddScoped<ISurveyService, SurveyService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IQuestionnaireService, QuestionnaireService>();

        // --- Repositories ---
        services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IWhitelistRepository, WhitelistRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        // --- Email Services ---
        // Email configuration: Loaded from environment variables
        services.AddSingleton<FeedBackApp.Core.Email.Configuration.EmailConfiguration>(
            _ => FeedBackApp.Core.Email.Configuration.EmailConfiguration.FromEnvironment());
        
        // Email content factory: Creates email messages based on recipient roles (Student, Teacher, Admin)
        services.AddScoped<IEmailContentFactory, EmailContentFactory>();
        
        // Email sender: MailKit-based SMTP implementation (replaces System.Net.Mail.SmtpClient)
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        
        // Email service: Orchestrates email batch processing and compilation
        services.AddScoped<IEmailService, EmailService>();

        // --- Report Services ---
        // Report service uses IBlobContext for blob storage operations
        services.AddScoped<IReportService, ReportService>();

        // --- Azure Functions ---
        services.AddScoped<QuestionnaireFunctions>();
        services.AddScoped<EvaluationFunctions>();
        services.AddScoped<ReportFunctions>();
        services.AddScoped<EmailSendingFunctions>();

        // Validators
        services.AddScoped<IValidator<CreateSurveyMetadataDTO>, CreateSurveyMetadataValidator>();
        services.AddScoped<IValidator<MetaTeacherDTO>, MetaTeacherValidator>();
        services.AddScoped<IValidator<QuestionnaireCreationParamDTO>, QuestionnaireCreationParamValidator>();
        services.AddScoped<IValidator<QuestionnaireDTO>, QuestionnaireValidator>();
        services.AddScoped<IValidator<QuestionTemplateDTO>, QuestionTemplateValidator>();
        services.AddScoped<IValidator<StudentSetDTO>, StudentSetValidator>();

        // Middleware
        services.AddSingleton<AdminOnlyMiddleware>();
        services.AddSingleton<StudentOnlyMiddleware>();
        services.AddSingleton<MiddlewareSelector>();
    })
    .ConfigureFunctionsWebApplication((IFunctionsWorkerApplicationBuilder app) =>
    {
        app.UseMiddleware<MiddlewareSelector>();
    })
    .Build();

// Inicializálás: csak DB (BlobContainer létrehozást a BlobContext intézi a konstruktorban)
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

await host.RunAsync();
