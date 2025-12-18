using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Context
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
    {
        public DbSet<SurveyMetadata> Surveys { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; }
        public DbSet<EmailsToSend> EmailsToSend { get; set; }
        public DbSet<StudentWhitelist> StudentWhitelist { get; set; }

        private readonly string _containerName = Environment.GetEnvironmentVariable("Cosmos:ContainerName")
                ?? throw new InvalidOperationException("Cosmos ContainerName not set in environment variables");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultContainer(_containerName);

            // basic setup
            modelBuilder.Entity<SurveyMetadata>()
                .ToContainer(_containerName)
                .HasPartitionKey(m => m.Id)
                .HasKey(m => m.Id);

            modelBuilder.Entity<Questionnaire>()
                .ToContainer(_containerName)
                .HasPartitionKey(q => q.Id)
                .HasKey(q => q.Id);

            modelBuilder.Entity<QuestionnaireTemplate>()
                .ToContainer(_containerName)
                .HasPartitionKey(q => q.Id)
                .HasKey(q => q.Id);

            modelBuilder.Entity<EmailsToSend>()
                .ToContainer(_containerName)
                .HasPartitionKey(q => q.Id)
                .HasKey(e => e.Id);

            modelBuilder.Entity<StudentWhitelist>()
                .ToContainer(_containerName)
                .HasPartitionKey(q => q.Id)
                .HasKey(e => e.Id);

            // discriminators
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

            modelBuilder.Entity<StudentWhitelist>()
                .HasDiscriminator<string>("DocumentType")
                .HasValue<StudentWhitelist>("StudentWhitelist");
        }
    }
}
