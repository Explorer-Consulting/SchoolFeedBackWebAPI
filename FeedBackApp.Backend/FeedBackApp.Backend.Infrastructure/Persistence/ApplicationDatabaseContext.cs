using Core.DomainModels;
using Microsoft.EntityFrameworkCore;
using DatabaseContext = Microsoft.EntityFrameworkCore.DbContext;
using DatabaseContextOptions = Microsoft.EntityFrameworkCore.DbContextOptions;
namespace Infrastructure.Persistence;

public sealed class ApplicationDatabaseContext(DatabaseContextOptions options) : DatabaseContext(options)
{
    public DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; }
    public DbSet<QuestionnaireResponse> QuestionnaireResponses { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDatabaseContext).Assembly);
    }
}