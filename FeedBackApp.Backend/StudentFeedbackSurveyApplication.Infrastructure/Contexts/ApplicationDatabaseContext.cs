using Microsoft.EntityFrameworkCore;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;

namespace StudentFeedbackSurveyApplication.Infrastructure.Contexts;

public sealed class ApplicationDatabaseContext(
    DbContextOptions<ApplicationDatabaseContext> options
) : DbContext(options)
{

    public DbSet<QuestionnaireTemplateDocument> QuestionnaireTemplateCollection { get; init; }
    public DbSet<QuestionnaireResponseDocument> QuestionnaireResponseCollection { get; init; }
    public DbSet<User> UserCollection { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDatabaseContext).Assembly);
    }
}
