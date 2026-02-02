using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

// under planning
namespace StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots
{
    /// <summary>
    /// Represents a user account within the system, including identification, contact information, and activity
    /// timestamps.
    /// </summary>
    public sealed class User : AggregateEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        /// <remarks>This property is typically used as a partition key in data storage scenarios. The
        /// value must be a valid ULID that uniquely identifies a user within the system.</remarks>
        public required ULID UserId { get; set; } // this is for partition key
        /// <summary>
        /// Gets or sets the unique identifier of the questionnaire template associated with this entity.
        /// </summary>
        public required ULID QuestionnaireTemplateId { get; set; }
        /// <summary>
        /// Gets or sets the email address associated with the entity.
        /// </summary>
        public required string Email { get; set; }
        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// </summary>
        public required DateTimeOffset CreateAt { get; set; }
        /// <summary>
        /// Gets or sets the date and time when the user last logged in.
        /// </summary>
        public required DateTimeOffset LastLoginAt { get; set; }

    }
}
