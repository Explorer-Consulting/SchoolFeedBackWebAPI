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

var host = new HostBuilder()
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        /// <summary>
        /// Adds layered configuration sources: base JSON, environment-specific JSON, and environment variables.
        /// </summary>
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true)
           .AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        /// <summary>
        /// Enables Application Insights telemetry for the isolated worker.
        /// </summary>
        services.AddApplicationInsightsTelemetryWorkerService();

        /// <summary>
        /// Configures worker JSON (camelCase) for request/response serialization.
        /// </summary>
        services.Configure<WorkerOptions>(o =>
        {
            o.Serializer = new JsonObjectSerializer(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        });

        /// <summary>
        /// Registers EF Core DbContext targeting Azure Cosmos DB using the ConnectionString env var.
        /// </summary>
        services.AddDbContext<AppDBContext>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString")
                ?? throw new InvalidOperationException("ConnectionString environment variable is not set.");

            options.UseCosmos(connectionString, databaseName: "SchoolDatabase");
        });

        /// <summary>
        /// Provides BlobServiceClient from AZURE_REPORT_BLOB_STORAGE for report artifacts.
        /// </summary>
        services.AddSingleton(sp =>
        {
            var cs = Environment.GetEnvironmentVariable("AZURE_REPORT_BLOB_STORAGE");
            if (!string.IsNullOrWhiteSpace(cs))
                return new BlobServiceClient(cs);

            throw new InvalidOperationException("Set AZURE_REPORT_BLOB_STORAGE.");
        });

        /// <summary>
        /// Binds IBlobContext to a concrete BlobContext with container from AZURE_REPORTS_CONTAINER.
        /// </summary>
        services.AddSingleton<IBlobContext>(sp =>
        {
            var svc = sp.GetRequiredService<BlobServiceClient>();
            var containerName = Environment.GetEnvironmentVariable("AZURE_REPORTS_CONTAINER")
                ?? throw new InvalidOperationException("AZURE_REPORTS_CONTAINER is not set.");
            return new BlobContext(svc, containerName); // container creation handled inside
        });

        /// <summary>
        /// Application services and repositories (scoped).
        /// </summary>
        services.AddScoped<ISurveyService, SurveyService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IWhitelistRepository, WhitelistRepository>();
        services.AddScoped<IQuestionnaireService, QuestionnaireService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailRepository, EmailRepository>();

        /// <summary>
        /// Reporting stack (uses IBlobContext under the hood).
        /// </summary>
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportService, ReportService>();

        /// <summary>
        /// HTTP function classes (DI-enabled).
        /// </summary>
        services.AddScoped<QuestionnaireFunctions>();
        services.AddScoped<EvaluationFunctions>();
        services.AddScoped<ReportFunctions>();

        /// <summary>
        /// FluentValidation validators for survey/questionnaire creation.
        /// </summary>
        services.AddScoped<IValidator<CreateSurveyMetadataDTO>, CreateSurveyMetadataValidator>();
        services.AddScoped<IValidator<MetaTeacherDTO>, MetaTeacherValidator>();
        services.AddScoped<IValidator<QuestionnaireCreationParamDTO>, QuestionnaireCreationParamValidator>();
        services.AddScoped<IValidator<QuestionnaireDTO>, QuestionnaireValidator>();
        services.AddScoped<IValidator<QuestionTemplateDTO>, QuestionTemplateValidator>();
        services.AddScoped<IValidator<StudentSetDTO>, StudentSetValidator>();

        /// <summary>
        /// Authorization/user-context middleware and selector.
        /// </summary>
        services.AddSingleton<AdminOnlyMiddleware>();
        services.AddSingleton<StudentOnlyMiddleware>();
        services.AddSingleton<MiddlewareSelector>();
    })
    .Build();

/// <summary>
/// Ensures the database is created before the host begins processing.
/// </summary>
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

/// <summary>
/// Starts the Azure Functions host.
/// </summary>
await host.RunAsync();
