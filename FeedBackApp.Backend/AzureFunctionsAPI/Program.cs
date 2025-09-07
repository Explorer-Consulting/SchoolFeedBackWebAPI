using Application.DTOs.Questionnaire;
using Application.DTOs.Survey;
using Application.Services;
using Application.Services.Interfaces;
using Application.Validation.CreateValidation;
using Azure.Core.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureFunctionsAPI.AzureEndPointReaction.Functions;
using FeedBackApp.Backend.Infrastructure.Middleware;
using FeedBackApp.Backend.Infrastructure.Middleware.Utils;
using FeedBackApp.Backend.Infrastructure.Persistence;
using FeedBackApp.Backend.Infrastructure.Persistence.Repository;
using FeedBackApp.Core.Repositories;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

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
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
        });

        // EF Core Cosmos konfiguráció
        services.AddDbContext<AppDBContext>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString")
                ?? throw new InvalidOperationException("ConnectionString environment variable is not set.");

            options.UseCosmos(
                connectionString: connectionString,
                databaseName: "SchoolDatabase"
            );
        });

        // BlobServiceClient regisztráció
        services.AddSingleton(sp =>
        {
            var blobConnectionString = Environment.GetEnvironmentVariable("AZURE_REPORT_BLOB_STORAGE")
                ?? throw new InvalidOperationException("AZURE_REPORT_BLOB_STORAGE environment variable is not set.");

            return new BlobServiceClient(blobConnectionString);
        });

        // BlobContainerClient regisztráció (pl. "reports" konténer)
        services.AddSingleton(sp =>
        {
            var blobService = sp.GetRequiredService<BlobServiceClient>();
            var containerName = Environment.GetEnvironmentVariable("AZURE_REPORTS_CONTAINER") ?? "reports";
            return blobService.GetBlobContainerClient(containerName);
        });

        // Applikációs szolgáltatások
        services.AddScoped<ISurveyService, SurveyService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IWhitelistRepository, WhitelistRepository>();
        services.AddScoped<IQuestionnaireService, QuestionnaireService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailRepository, EmailRepository>();

        // Riport repository/service
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportService, ReportService>();

        // Function osztályok
        services.AddScoped<QuestionnaireFunctions>();
        services.AddScoped<EvaluationFunctions>();

        // Validátorok
        services.AddScoped<IValidator<CreateSurveyMetadataDTO>, CreateSurveyMetadataValidator>();
        services.AddScoped<IValidator<MetaTeacherDTO>, MetaTeacherValidator>();
        services.AddScoped<IValidator<QuestionnaireCreationParamDTO>, QuestionnaireCreationParamValidator>();
        services.AddScoped<IValidator<QuestionnaireDTO>, QuestionnaireValidator>();
        services.AddScoped<IValidator<QuestionTemplateDTO>, QuestionTemplateValidator>();
        services.AddScoped<IValidator<StudentSetDTO>, StudentSetValidator>();

        // Middleware-ek
        services.AddSingleton<AdminOnlyMiddleware>();
        services.AddSingleton<StudentOnlyMiddleware>();
        services.AddSingleton<MiddlewareSelector>();
    })
    .ConfigureFunctionsWebApplication((IFunctionsWorkerApplicationBuilder app) =>
    {
        app.UseMiddleware<MiddlewareSelector>();
    })
    .Build();

// Inicializáció: DB + Blob konténer
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var container = scope.ServiceProvider.GetRequiredService<BlobContainerClient>();
    await container.CreateIfNotExistsAsync(PublicAccessType.None);
}

await host.RunAsync();
