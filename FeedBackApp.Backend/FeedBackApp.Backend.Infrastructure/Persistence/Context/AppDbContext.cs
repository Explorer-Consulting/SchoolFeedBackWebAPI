using System.Globalization;
using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Context
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
    /// Entity Framework Core DbContext configured for Azure Cosmos DB (EF Core provider).
    /// </summary>
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

            // All entities mapped to the same Cosmos container
            modelBuilder.HasDefaultContainer(_containerName);

            // --- Entity mappings & partition keys ---
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
                .HasPartitionKey(e => e.Id)
                .HasKey(e => e.Id);

            modelBuilder.Entity<StudentWhitelist>()
                .ToContainer(_containerName)
                .HasPartitionKey(s => s.Id)
                .HasKey(s => s.Id);

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
            // --- DateTime <-> string conversion for Cosmos (StartDate / EndDate) ---
            var dtConverter = new ValueConverter<DateTime, string>(
                v => v.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture), // ISO "O" with Z
                s => DateTime.SpecifyKind(
                        DateTime.Parse(
                            (s ?? string.Empty).Trim('\"'), // handle accidentally quoted values
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                        DateTimeKind.Utc)
            );

            modelBuilder.Entity<SurveyMetadata>()
                .Property(x => x.StartDate)
                .HasConversion(dtConverter);

            modelBuilder.Entity<SurveyMetadata>()
                .Property(x => x.EndDate)
                .HasConversion(dtConverter);

            // --- Unblock: ignore list properties stored with inconsistent shapes in Cosmos ---
            modelBuilder.Entity<SurveyMetadata>()
                .Ignore(x => x.Teachers)
                .Ignore(x => x.StudentSets)
                .Ignore(x => x.QuestionTemplates)
                .Ignore(x => x.CreationParams);
        }
    }
}
