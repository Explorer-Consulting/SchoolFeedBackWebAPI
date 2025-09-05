using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence
{
    public class AppDBContext : DbContext
    {
        public DbSet<SurveyMetadata> Surveys { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; }
        public DbSet<EmailsToSend> EmailsToSend { get; set; }

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultContainer("surveyContainer");

            modelBuilder.Entity<SurveyMetadata>()
                .ToContainer("surveyContainer")
                .HasPartitionKey(m => m.Id)
                .HasKey(m => m.Id);

            modelBuilder.Entity<Questionnaire>()
                .ToContainer("surveyContainer")
                .HasPartitionKey(q => q.Id)
                .HasKey(q => q.Id);

            modelBuilder.Entity<QuestionnaireTemplate>()
                .ToContainer("surveyContainer")
                .HasPartitionKey(q => q.Id)
                .HasKey(q => q.Id);

            modelBuilder.Entity<EmailsToSend>()
                .ToContainer("surveyContainer")
                .HasPartitionKey(q => q.Id)
                .HasKey(e => e.Id);

            modelBuilder.Entity<SurveyMetadata>()
                .HasDiscriminator<string>("DocumentType")
                .HasValue<SurveyMetadata>("Survey");

            modelBuilder.Entity<Questionnaire>()
                .HasDiscriminator<string>("DocumentType")
                .HasValue<Questionnaire>("Questionnaire");

            modelBuilder.Entity<QuestionnaireTemplate>()
                .HasDiscriminator<string>("DocumentType")
                .HasValue<QuestionnaireTemplate>("QuestionTemplate");

            modelBuilder.Entity<EmailsToSend>()
                .HasDiscriminator<string>("DocumentType")
                .HasValue<EmailsToSend>("EmailsToSend");

        }
    }
}
