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
//megy
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
// 5) Egyéb config ellenőrzés (JWT, Google, Email, AdminEmails)
// ─────────────────────────────────────────────────────
_ = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Missing Jwt:SecretKey");

_ = builder.Configuration["Google:ClientId"]
    ?? throw new InvalidOperationException("Missing Google:ClientId");

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
builder.Services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddScoped<IWhitelistRepository, WhitelistRepository>();
builder.Services.AddScoped<IQuestionnaireService, QuestionnaireService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();

builder.Services.AddScoped<IReportRepository, ReportRepository>();
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