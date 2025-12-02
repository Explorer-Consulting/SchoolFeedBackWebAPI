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

/// <summary>
/// Bootstraps the Azure Functions (.NET isolated worker) host for the School Feedback application.
/// </summary>
/// <remarks>
/// <para>
/// <b>Configuration pipeline</b><br/>
/// Loads hierarchical configuration from <c>appsettings.json</c>, environment-specific overrides
/// (<c>appsettings.&lt;Environment&gt;.json</c>), and environment variables. Settings are made
/// available to the DI container and framework components.
/// </para>
/// <para>
/// <b>Serialization</b><br/>
/// Configures the Functions worker to use <see cref="JsonObjectSerializer"/> with camelCase
/// property naming to ensure consistent request/response payload shapes across endpoints.
/// </para>
/// <para>
/// <b>Observability</b><br/>
/// Registers Application Insights worker telemetry for traces, metrics, and logs emitted
/// by Functions and services.
/// </para>
/// <para>
/// <b>Data layer (Cosmos DB via EF Core)</b><br/>
/// Registers <see cref="AppDBContext"/> against Azure Cosmos DB using the
/// <c>ConnectionString</c> environment variable and the logical database name <c>SchoolDatabase</c>.
/// Database creation is ensured during startup via <see cref="DbContext.Database.EnsureCreatedAsync()"/>.
/// </para>
/// <para>
/// <b>Blob storage</b><br/>
/// Provides a singleton <see cref="BlobServiceClient"/> using <c>AZURE_REPORT_BLOB_STORAGE</c>.
/// Binds an <see cref="IBlobContext"/> backed by a container named via <c>AZURE_REPORTS_CONTAINER</c>.
/// The container is created (if missing) in the <see cref="BlobContext"/> constructor,
/// enabling report artifact storage.
/// </para>
/// <para>
/// <b>Domain services &amp; repositories</b><br/>
/// Wires application services (<see cref="ISurveyService"/>, <see cref="IEvaluationService"/>,
/// <see cref="IQuestionnaireService"/>, <see cref="IEmailService"/>, <see cref="IReportService"/>)
/// and their repositories (questionnaire, evaluation, whitelist, email, report) with scoped lifetimes.
/// </para>
/// <para>
/// <b>HTTP Functions</b><br/>
/// Registers the function classes (<see cref="QuestionnaireFunctions"/>, <see cref="EvaluationFunctions"/>,
/// <see cref="ReportFunctions"/>), enabling DI for their dependencies.
/// </para>
/// <para>
/// <b>Validation</b><br/>
/// Adds FluentValidation validators for survey/questionnaire creation DTOs to enforce
/// input integrity at the application boundary.
/// </para>
/// <para>
/// <b>Middleware</b><br/>
/// Configures custom middleware for authorization and user-context population:
/// <list type="bullet">
///   <item><description><see cref="AdminOnlyMiddleware"/></description></item>
///   <item><description><see cref="StudentOnlyMiddleware"/></description></item>
///   <item><description><see cref="MiddlewareSelector"/> (entry point that routes to the appropriate guard)</description></item>
/// </list>
/// The selector is inserted into the Functions pipeline via <see cref="IFunctionsWorkerApplicationBuilder.UseMiddleware{T}"/>.
/// </para>
/// <para>
/// <b>Host lifecycle</b><br/>
/// Ensures data store readiness, then starts the Functions host to process triggers.
/// </para>
/// <para>
/// <b>Licensing</b><br/>
/// Initializes QuestPDF community license for document generation features used by reporting.
/// </para>
/// </remarks>

// testing commit validation finally works
// testing commit validation from user interface
QuestPDF.Settings.License = LicenseType.Community;

// ──────────────────────────────────────────────────---
// 1) Modern isolated builder
// ─────────────────────────────────────────────────────
var builder = FunctionsApplication.CreateBuilder(args);
        /// Adds layered configuration sources: base JSON, environment-specific JSON, and environment variables.
        /// </summary>
        /// <summary>
        /// Enables Application Insights telemetry for the isolated worker.
        /// </summary>

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
        /// Binds IBlobContext to a concrete BlobContext with container from AZURE_REPORTS_CONTAINER.
        /// </summary>
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
        /// </summary>

var rawAdminEmails = builder.Configuration["AdminEmails"] ?? string.Empty;
var adminEmails = rawAdminEmails
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

_ = builder.Configuration["Email:FromAddress"]
    ?? throw new InvalidOperationException("Missing Email:FromAddress");
        /// </summary>

_ = builder.Configuration["Email:FromName"]
    ?? throw new InvalidOperationException("Missing Email:FromName");
        /// </summary>
        /// <summary>
        /// Inserts the middleware selector into the Functions request pipeline.
        /// </summary>

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
/// Starts the Azure Functions host.
/// </summary>