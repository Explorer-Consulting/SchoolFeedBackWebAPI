using Core.Interfaces;
using Infrastructure.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDatabaseContext>(options =>
{
    options.UseCosmos(
        accountEndpoint: builder.Configuration["CosmosDB:AccountEndpoint"]!,
        accountKey: builder.Configuration["CosmosDB:AccountKey"]!,
        databaseName: builder.Configuration["CosmosDB:DatabaseName"]!);
});
builder.Services.AddSingleton<IQuestionnaireResponseAggregateRepository, CosmosQuestionnaireResponseRepository>();
builder.Services.AddSingleton<IQuestionnaireTemplateAggregateRepository, CosmosQuestionnaireTemplateRepository>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
