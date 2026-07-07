using FeedBackApp.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedBackApp.Backend.Infrastructure.Persistence.DocumentConfigurations;

public sealed class QuestionnaireTemplateConfiguration : IEntityTypeConfiguration<QuestionnaireTemplate>
{
    public required string ContainerName { get; init; }

    public void Configure(EntityTypeBuilder<QuestionnaireTemplate> builder)
    {
        builder.ToContainer(ContainerName);

        builder.HasKey(x => x.Id);
        builder.HasPartitionKey(x => x.Id);

        builder.Property(x => x.Id)
            .ToJsonProperty("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasDiscriminator<string>("DocumentType")
            .HasValue("QuestionTemplate");

        builder.Property(x => x.Title)
            .ToJsonProperty("title")
            .IsRequired();

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
    }
}
