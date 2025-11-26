using Application.DTOs.Questionnaire;
using Application.DTOs.Survey;
using Application.Services;
using Application.Services.Interfaces;
using Application.Validation.CreateValidation;
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
using QuestPDF.Infrastructure;
using System.Text.Json;

QuestPDF.Settings.License = LicenseType.Community;

// ─────────────────────────────────────────────────────
// 1) Modern isolated builder
// ─────────────────────────────────────────────────────
var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// ─────────────────────────────────────────────────────
// 2) Worker JSON serializer
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
// 3) EF Core Cosmos – config közvetlenül local.settings.json-ból
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
            "Missing Cosmos configuration values. Check local.settings.json.");
    }

    options.UseCosmos(endpoint, key, db);
});

// ─────────────────────────────────────────────────────
// 4) Blob Storage – szintén közvetlenül configból, plusz osztály nélkül
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
// 5) Services & repositories
// ─────────────────────────────────────────────────────
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();
builder.Services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddScoped<IWhitelistRepository, WhitelistRepository>();
builder.Services.AddScoped<IQuestionnaireService, QuestionnaireService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();

builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

// ─────────────────────────────────────────────────────
// 6) Functions injection
// ─────────────────────────────────────────────────────
builder.Services.AddScoped<QuestionnaireFunctions>();
builder.Services.AddScoped<EvaluationFunctions>();
builder.Services.AddScoped<ReportFunctions>();

// ─────────────────────────────────────────────────────
// 7) Validators
// ─────────────────────────────────────────────────────
builder.Services.AddScoped<IValidator<CreateSurveyMetadataDTO>, CreateSurveyMetadataValidator>();
builder.Services.AddScoped<IValidator<MetaTeacherDTO>, MetaTeacherValidator>();
builder.Services.AddScoped<IValidator<QuestionnaireCreationParamDTO>, QuestionnaireCreationParamValidator>();
builder.Services.AddScoped<IValidator<QuestionnaireDTO>, QuestionnaireValidator>();
builder.Services.AddScoped<IValidator<QuestionTemplateDTO>, QuestionTemplateValidator>();
builder.Services.AddScoped<IValidator<StudentSetDTO>, StudentSetValidator>();

// ─────────────────────────────────────────────────────
// 8) Middleware
// ─────────────────────────────────────────────────────
builder.Services.AddSingleton<AdminOnlyMiddleware>();
builder.Services.AddSingleton<StudentOnlyMiddleware>();
builder.Services.AddSingleton<MiddlewareSelector>();

builder.ConfigureFunctionsWebApplication(app =>
{
    app.UseMiddleware<MiddlewareSelector>();
});

// ─────────────────────────────────────────────────────
// 9) Build, DB init, run
// ─────────────────────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
