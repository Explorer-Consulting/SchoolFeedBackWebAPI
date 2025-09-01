using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence
{
    public class AppDBContext : DbContext
    {
        public DbSet<SurveyMetadata> Surveys { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; }

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultContainer("mainContainer");

            modelBuilder.Entity<SurveyMetadata>()
                .ToContainer("mainContainer")
                .HasPartitionKey(m => m.Id)
                .HasKey(m => m.Id);

            modelBuilder.Entity<Questionnaire>()
                .ToContainer("mainContainer")
                .HasPartitionKey(q => q.Id)
                .HasKey(q => q.Id);

            modelBuilder.Entity<QuestionnaireTemplate>()
                .ToContainer("mainContainer")
                .HasPartitionKey(q => q.Id)
                .HasKey(q => q.Id);
            
            modelBuilder.Entity<SurveyMetadata>()
                .HasDiscriminator<string>("DocumentType")
                .HasValue<SurveyMetadata>("Survey");

            modelBuilder.Entity<Questionnaire>()
                .HasDiscriminator<string>("DocumentType")
                .HasValue<Questionnaire>("Questionnaire");

            modelBuilder.Entity<QuestionnaireTemplate>()
                .HasDiscriminator<string>("DocumentType")
                .HasValue<QuestionnaireTemplate>("QuestionTemplate");

        }

    }
}
