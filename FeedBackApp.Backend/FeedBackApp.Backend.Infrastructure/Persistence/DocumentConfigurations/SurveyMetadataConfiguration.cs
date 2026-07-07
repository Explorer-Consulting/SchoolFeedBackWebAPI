using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedBackApp.Backend.Infrastructure.Persistence.DocumentConfigurations;

public sealed class SurveyMetadataConfiguration : IEntityTypeConfiguration<SurveyMetadata>
{
    public required string ContainerName { get; init; }

    public void Configure(EntityTypeBuilder<SurveyMetadata> builder)
    {
        builder.ToContainer(ContainerName);

        builder.HasKey(x => x.Id);
        builder.HasPartitionKey(x => x.Id);

        builder.Property(x => x.Id)
            .ToJsonProperty("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Title)
            .ToJsonProperty("title")
            .IsRequired();

        builder.Property(x => x.StartDate).ToJsonProperty("startDate");
        builder.Property(x => x.EndDate).ToJsonProperty("endDate");

        builder.HasDiscriminator<string>("DocumentType")
            .HasValue("Survey");

        // StudentSets (owned collection)
        builder.OwnsMany(x => x.StudentSets, ss =>
        {
            ss.ToJsonProperty("studentSets");

            ss.Property(p => p.SetId).ToJsonProperty("setId").IsRequired();

            ss.Property(p => p.StudentEmails).ToJsonProperty("studentEmails");
        });

        // QuestionTemplates (owned collection)
        builder.OwnsMany(x => x.QuestionTemplates, qt =>
        {
            qt.ToJsonProperty("questionTemplates");

            qt.Property(p => p.Id).ToJsonProperty("id").IsRequired();
            qt.Property(p => p.Question).ToJsonProperty("question").IsRequired();
            qt.Property(p => p.Type).ToJsonProperty("type");
            qt.Property(p => p.Category).ToJsonProperty("category").IsRequired();
            qt.Property(p => p.Description).ToJsonProperty("description");
            qt.Property(p => p.AnswerOptions).ToJsonProperty("answerOptions");
            qt.Property(p => p.RequiredValidation).ToJsonProperty("requiredValidation");

            qt.OwnsOne(p => p.Dependency, dep =>
            {
                dep.ToJsonProperty("dependency");
                dep.Property(d => d.Id).ToJsonProperty("id");
                dep.Property(d => d.AnswerConditions).ToJsonProperty("answerConditions");
            });
        });

        // Teachers (owned collection)
        builder.OwnsMany(x => x.Teachers, t =>
        {
            t.ToJsonProperty("teachers");
            t.Property(p => p.Email).ToJsonProperty("email").IsRequired();
            t.Property(p => p.Name).ToJsonProperty("name").IsRequired();
        });

        // CreationParams (owned collection)
        builder.OwnsMany(x => x.CreationParams, cp =>
        {
            cp.ToJsonProperty("creationParams");

            cp.Property(p => p.TeacherEmail).ToJsonProperty("teacherEmail").IsRequired();
            cp.Property(p => p.SubjectName).ToJsonProperty("subjectName").IsRequired();
            cp.Property(p => p.StudentSetIds).ToJsonProperty("studentSetIds");
        });
    }
}
