using FeedBackApp.Backend.Infrastructure.Persistence.DocumentConfigurations;
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

            // All entities mapped to the same Cosmos container
            modelBuilder.HasDefaultContainer(_containerName);


            modelBuilder.ApplyConfiguration(new SurveyMetadataConfiguration { ContainerName = _containerName });
            modelBuilder.ApplyConfiguration(new QuestionnaireConfiguration { ContainerName = _containerName });
            modelBuilder.ApplyConfiguration(new QuestionnaireTemplateConfiguration { ContainerName = _containerName });
            modelBuilder.ApplyConfiguration(new EmailsToSendConfiguration { ContainerName = _containerName });
            modelBuilder.ApplyConfiguration(new StudentWhiteListConfiguration { ContainerName = _containerName });

            /*
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
            */
            /*
            modelBuilder.Entity<SurveyMetadata>()
                .HasDiscriminator<string>("DocumentType");

            modelBuilder.Entity<Questionnaire>()
                .HasDiscriminator<string>("DocumentType");

            modelBuilder.Entity<QuestionnaireTemplate>()
                .HasDiscriminator<string>("DocumentType");

            modelBuilder.Entity<EmailsToSend>()
                .HasDiscriminator<string>("DocumentType");

            modelBuilder.Entity<StudentWhitelist>()
                .HasDiscriminator<string>("DocumentType");
            */
            
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
