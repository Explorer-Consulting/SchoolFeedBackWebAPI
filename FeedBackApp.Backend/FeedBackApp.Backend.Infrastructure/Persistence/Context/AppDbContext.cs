using FeedBackApp.Backend.Infrastructure.Persistence.Helpers;
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
        public DbSet<StudentWhitelist> StudentWhitelist { get; set; }

        private readonly string _containerName;
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) {
                _containerName = Environment.GetEnvironmentVariable("COSMOS_CONTAINER_NAME")
                    ?? throw new InvalidOperationException("COSMOS_CONTAINER_NAME not set in environment variables");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultContainer("surveyContainer");

            // basic setup
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

            modelBuilder.Entity<StudentWhitelist>()
                .ToContainer("surveyContainer")
                .HasPartitionKey(q => q.Id)
                .HasKey(e => e.Id);

            // encrypting
            modelBuilder.Entity<SurveyMetadata>()
                .Property(s => s.Title)
                .HasConversion(new RecursiveConverter<string>());

            modelBuilder.Entity<SurveyMetadata>()
                .Property(s => s.StartDate)
                .HasConversion(new RecursiveConverter<DateTime>());

            modelBuilder.Entity<SurveyMetadata>()
                .Property(s => s.EndDate)
                .HasConversion(new RecursiveConverter<DateTime>());

            modelBuilder.Entity<SurveyMetadata>()
                .Property(s => s.StudentSets)
                .HasConversion(new RecursiveConverter<IList<StudentSet>>());

            modelBuilder.Entity<SurveyMetadata>()
                .Property(s => s.QuestionTemplates)
                .HasConversion(new RecursiveConverter<IList<QuestionTemplate>>());

            modelBuilder.Entity<SurveyMetadata>()
                .Property(s => s.Teachers)
                .HasConversion(new RecursiveConverter<IList<MetaTeacher>>());

            modelBuilder.Entity<SurveyMetadata>()
                .Property(s => s.CreationParams)
                .HasConversion(new RecursiveConverter<IList<QuestionnaireCreationParam>>());

            modelBuilder.Entity<QuestionnaireTemplate>()
                .Property(q => q.QuestionTemplates)
                .HasConversion(new RecursiveConverter<IList<QuestionTemplate>>());

            modelBuilder.Entity<Questionnaire>()
                .Property(q => q.Status)
                .HasConversion(new RecursiveConverter<bool>());

            modelBuilder.Entity<Questionnaire>()
                .Property(q => q.QuestionnaireResults)
                .HasConversion(new RecursiveConverter<IList<QuestionAnswer>>());

            modelBuilder.Entity<StudentWhitelist>()
                .Property(w => w.StudentEmails)
                .HasConversion(new RecursiveConverter<List<string>>());

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
