using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Context
{
    /// <summary>
    /// Entity Framework Core DbContext configured for Azure Cosmos DB (EF Core provider).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Container strategy</b><br/>
    /// All aggregates are stored in a single logical Cosmos container, whose name is read from the
    /// <c>COSMOS_CONTAINER_NAME</c> environment variable. Each CLR type has a discriminator value
    /// (<c>DocumentType</c>) to enable single-container polymorphism.
    /// </para>
    ///
    /// <para>
    /// <b>Partitioning</b><br/>
    /// Each entity type uses its primary key (<c>Id</c>) as the partition key in this configuration,
    /// implying one logical partition per entity. This maximizes locality for single-entity operations
    /// but does not group related aggregates; adjust if cross-entity partition colocation is desired.
    /// </para>
    ///
    /// <para>
    /// <b>Value conversion (encryption-ready)</b><br/>
    /// Complex and scalar properties are routed through <c>RecursiveConverter&lt;T&gt;</c> via
    /// <see cref="PropertyBuilder.HasConversion(ValueConverter)"/>. The converter is intended to
    /// serialize/deserialize (and optionally encrypt/decrypt) values transparently at the EF boundary.
    /// For collections, a <see cref="ValueComparer{T}"/> is registered to ensure EF change tracking
    /// uses semantic equality rather than reference identity.
    /// </para>
    ///
    /// <para>
    /// <b>Discriminators</b><br/>
    /// The <c>DocumentType</c> discriminator is set for each entity to allow storing multiple CLR types
    /// in the same container while preserving correct materialization.
    /// </para>
    /// </remarks>
    public class AppDBContext : DbContext
    {
        /// <summary>Survey metadata documents.</summary>
        public DbSet<SurveyMetadata> Surveys { get; set; }

        /// <summary>Student-teacher-subject questionnaires (per student &amp; subject).</summary>
        public DbSet<Questionnaire> Questionnaires { get; set; }

        /// <summary>Question templates bound to a survey.</summary>
        public DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; }

        /// <summary>Outbox-style document for pending outbound emails.</summary>
        public DbSet<EmailsToSend> EmailsToSend { get; set; }

        /// <summary>Whitelist of student email addresses authorized to use the app.</summary>
        public DbSet<StudentWhitelist> StudentWhitelist { get; set; }

        private readonly string _containerName;

        /// <summary>
        /// Initializes a new instance of the context, resolving the Cosmos container name from environment variables.
        /// </summary>
        /// <param name="options">EF Core options configured by the host.</param>
        /// <exception cref="InvalidOperationException">Thrown when <c>COSMOS_CONTAINER_NAME</c> is not set.</exception>
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
            _containerName = Environment.GetEnvironmentVariable("COSMOS_CONTAINER_NAME")
                ?? throw new InvalidOperationException("COSMOS_CONTAINER_NAME not set in environment variables");
        }

        /// <summary>
        /// Configures entity-to-container mappings, partition keys, discriminators, and value conversions.
        /// </summary>
        /// <param name="modelBuilder">Fluent API builder for EF Core.</param>
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
                .HasPartitionKey(q => q.Id)
                .HasKey(e => e.Id);

            modelBuilder.Entity<StudentWhitelist>()
                .ToContainer(_containerName)
                .HasPartitionKey(q => q.Id)
                .HasKey(e => e.Id);

            // --- Value conversions (encryption/serialization-ready) ---
            // SurveyMetadata scalar/complex properties
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

            // QuestionnaireTemplate properties
            modelBuilder.Entity<QuestionnaireTemplate>()
                .Property(q => q.QuestionTemplates)
                .HasConversion(new RecursiveConverter<IList<QuestionTemplate>>());

            modelBuilder.Entity<QuestionnaireTemplate>()
                .Property(q => q.Title)
                .HasConversion(new RecursiveConverter<string>());

            // Questionnaire properties
            modelBuilder.Entity<Questionnaire>()
                .Property(q => q.Status)
                .HasConversion(new RecursiveConverter<bool>());

            modelBuilder.Entity<Questionnaire>()
                .Property(q => q.QuestionnaireResults)
                .HasConversion(new RecursiveConverter<IList<QuestionAnswer>>())
                .Metadata.SetValueComparer(
                    // Ensure EF compares value snapshots semantically for change tracking
                    new ValueComparer<IList<QuestionAnswer>>(
                        (c1, c2) => JsonConvert.SerializeObject(c1) == JsonConvert.SerializeObject(c2),
                        c => JsonConvert.SerializeObject(c).GetHashCode(),
                        c => JsonConvert.DeserializeObject<IList<QuestionAnswer>>(JsonConvert.SerializeObject(c))!
                    )
                );

            // StudentWhitelist property
            modelBuilder.Entity<StudentWhitelist>()
                .Property(w => w.StudentEmails)
                .HasConversion(new RecursiveConverter<List<string>>());

            // --- Discriminators (single-container polymorphism) ---
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
