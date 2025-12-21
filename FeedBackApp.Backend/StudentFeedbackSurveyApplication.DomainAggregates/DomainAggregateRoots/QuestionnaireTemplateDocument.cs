using StudentFeedbackSurveyApplication.Domain.AggregateComponents;
using ULID = NUlid.Ulid;

namespace StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots
{
    /// <summary>
    /// Represents a questionnaire template document containing metadata and section definitions for a questionnaire.
    /// </summary>
    /// <remarks>This class is typically used to store and transfer the structure and configuration of a
    /// questionnaire, including its title, description, enrollment settings, active period, and the collection of
    /// category sections that define its content. Instances of this class are immutable after initialization and are
    /// intended for use as data transfer objects.</remarks>
    public sealed class QuestionnaireTemplateDocument : AggregateEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public required ULID Id { get; set; }
        /// <summary>
        /// Gets or sets the partition key that uniquely identifies the logical partition for the entity.
        /// </summary>
        public required ULID PartitionKey { get; set; }
        /// <summary>
        /// Gets or sets the title associated with the object.
        /// </summary>
        public required string Title { get; set; }
        /// <summary>
        /// Gets or sets the description associated with the object.
        /// </summary>
        public required string? Description { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether users are permitted to enroll themselves without administrator
        /// approval.
        /// </summary>
        public required bool SelfEnrollmentAllowed { get; set; }
        /// <summary>
        /// Gets or sets the start date and time for the associated event or period.
        /// </summary>
        public required DateTimeOffset StartDate { get; set; }
        /// <summary>
        /// Gets or sets the end date and time for the period represented by this instance.
        /// </summary>
        public required DateTimeOffset EndDate { get; set; }
        /// <summary>
        /// Gets or sets the collection of category sections associated with this instance.
        /// </summary>
        public required IReadOnlyList<CategorySection> CategorySections { get; set; }
    }
}
