using Application.DTOs.Questionnaire;
using Application.DTOs.Survey;
using Application.Services;
using Application.Services.Interfaces;
using Application.Validation.CreateValidation;
using Azure.Core.Serialization;
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
        // ide az appsettings helyere a vegleges konfiguracios file kellene bekeruljon. Ez csak pelda!!!!
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

        services.AddDbContext<AppDBContext>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString")
    ?? throw new InvalidOperationException("ConnectionString environment variable is not set.");
            options.UseCosmos(
                connectionString: connectionString,
                databaseName: "SchoolDatabase"
            );
        });

        
        services.AddScoped<ISurveyService, SurveyService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IWhitelistRepository, WhitelistRepository>();
        services.AddScoped<IQuestionnaireService, QuestionnaireService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<QuestionnaireFunctions>();
        services.AddScoped<EvaluationFunctions>();

        services.AddScoped<IValidator<CreateSurveyMetadataDTO>, CreateSurveyMetadataValidator>();
        services.AddScoped<IValidator<MetaTeacherDTO>, MetaTeacherValidator>();
        services.AddScoped<IValidator<QuestionnaireCreationParamDTO>, QuestionnaireCreationParamValidator>();
        services.AddScoped<IValidator<QuestionnaireDTO>, QuestionnaireValidator>();
        services.AddScoped<IValidator<QuestionTemplateDTO>, QuestionTemplateValidator>();
        services.AddScoped<IValidator<StudentSetDTO>, StudentSetValidator>();


        services.AddSingleton<AdminOnlyMiddleware>();
        services.AddSingleton<StudentOnlyMiddleware>();
        services.AddSingleton<MiddlewareSelector>();
    })
    .ConfigureFunctionsWebApplication((IFunctionsWorkerApplicationBuilder app) =>
    {
        app.UseMiddleware<MiddlewareSelector>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

await host.RunAsync();
