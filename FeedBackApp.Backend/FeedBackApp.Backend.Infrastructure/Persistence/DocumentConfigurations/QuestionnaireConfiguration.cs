using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedBackApp.Backend.Infrastructure.Persistence.DocumentConfigurations;

public sealed class QuestionnaireConfiguration : IEntityTypeConfiguration<Questionnaire>
{
    public required string ContainerName { get; init; }

    public void Configure(EntityTypeBuilder<Questionnaire> builder)
    {
        builder.ToContainer(ContainerName);

        builder.HasKey(x => x.Id);
        builder.HasPartitionKey(x => x.Id);

        builder.Property(x => x.Id)
            .ToJsonProperty("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasDiscriminator<string>("DocumentType")
            .HasValue("Questionnaire");

        builder.Property(x => x.Status).ToJsonProperty("status");
        builder.Property(x => x.SurveyId).ToJsonProperty("surveyId").IsRequired();
        builder.Property(x => x.TeacherEmail).ToJsonProperty("teacherEmail").IsRequired();
        builder.Property(x => x.StudentEmail).ToJsonProperty("studentEmail").IsRequired();
        builder.Property(x => x.SubjectName).ToJsonProperty("subjectName").IsRequired();
        builder.Property(x => x.IsValidate).ToJsonProperty("isValidate").IsRequired();
        builder.Property(x => x.ValidationToken).ToJsonProperty("validationToken");
        builder.OwnsMany(x => x.QuestionnaireResults, qa =>
        {
            qa.ToJsonProperty("questionnaireResults");

            qa.Property(p => p.QuestionId).ToJsonProperty("questionId").IsRequired();
            qa.Property(p => p.Answer).ToJsonProperty("answer").IsRequired();
        });
    }
}
