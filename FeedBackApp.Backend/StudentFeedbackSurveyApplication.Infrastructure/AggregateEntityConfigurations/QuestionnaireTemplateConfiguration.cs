using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;
using ULID = NUlid.Ulid;

namespace StudentFeedbackSurveyApplication.Infrastructure.AggregateEntityConfigurations
{
    public sealed class QuestionnaireTemplateConfiguration : IEntityTypeConfiguration<QuestionnaireTemplateDocument>
    {
        public required string ContainerName { get; init; } = "QuestionnaireTemplates";

        public void Configure(EntityTypeBuilder<QuestionnaireTemplateDocument> builder)
        {
            builder.ToContainer(ContainerName);
            builder.HasNoDiscriminator();

            builder.HasKey(q => q.Id);
            builder.HasPartitionKey(q => q.Id);

            builder.Property(x => x.Id)
                .HasConversion(v => v.ToString(), v => ULID.Parse(v))
                .ToJsonProperty("id")
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(q => q.Title)
                .ToJsonProperty("title")
                .IsRequired();

            builder.Property(q => q.Description)
                .ToJsonProperty("description")
                .IsRequired(false);

            builder.Property(q => q.SelfEnrollmentAllowed)
                .ToJsonProperty("selfEnrollmentAllowed")
                .IsRequired();

            builder.Property(q => q.StartDate)
                .ToJsonProperty("startDate")
                .IsRequired();

            builder.Property(q => q.EndDate)
                .ToJsonProperty("endDate")
                .IsRequired();

            builder.OwnsMany(q => q.CategorySections, section =>
            {
                section.ToJsonProperty("categorySections");

                section.Property(s => s.CategoryName)
                    .ToJsonProperty("categoryName")
                    .IsRequired();

                section.HasKey(s => s.CategoryName);

                section.OwnsMany(s => s.QuestionTemplateComponents, component =>
                {
                    component.ToJsonProperty("questionTemplateComponents");

                    component.Property(c => c.OrderNumber)
                        .ToJsonProperty("orderNumber")
                        .IsRequired();

                    component.HasKey(c => c.OrderNumber);

                    component.Property(c => c.Statement)
                        .ToJsonProperty("statement")
                        .IsRequired();

                    component.Property(c => c.Type)
                        .ToJsonProperty("type")
                        .IsRequired();

                    component.Property(c => c.AnswerOptions)
                        .ToJsonProperty("answerOptions")
                        .IsRequired();

                    component.OwnsMany(c => c.Dependencies, dep =>
                    {
                        dep.ToJsonProperty("dependencies");

                        dep.Property(d => d.DependencyOrderNumber)
                            .ToJsonProperty("dependencyOrderNumber")
                            .IsRequired();

                        dep.HasKey(d => d.DependencyOrderNumber);

                        dep.Property(d => d.AllowedValues)
                            .ToJsonProperty("allowedValues")
                            .IsRequired();
                    });
                });
            });
        }
    }
}
