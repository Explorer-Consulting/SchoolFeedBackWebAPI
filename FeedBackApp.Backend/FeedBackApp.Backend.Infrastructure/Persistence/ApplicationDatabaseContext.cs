using Microsoft.EntityFrameworkCore;
using DatabaseContext = Microsoft.EntityFrameworkCore.DbContext;
using DatabaseContextOptions = Microsoft.EntityFrameworkCore.DbContextOptions;
namespace Infrastructure.Persistence;

public sealed class ApplicationDatabaseContext(DatabaseContextOptions options) : DatabaseContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDatabaseContext).Assembly);
    }
}