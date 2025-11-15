using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace FeedBackApp.Core.DomainModels.QuestionnaireTemplateModels
{
    /// <summary>
    /// represents a single questionnaire template's metadata
    /// </summary>
    public sealed class QuestionnaireTemplateAttributeHeader
    {
        public ULID TrafficId { get; init; } = ULID.NewUlid();

        public ULID StorageId { get; init; } = ULID.NewUlid();

        public required string Title { get; init; }

        public string? Description { get; init; }
        public required DateTimeOffset StartDate { get; init; }

        public required DateTimeOffset EndDate { get; init; }

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
        
        public  DateTimeOffset? UpdatedAt { get; init; }

        public DateTimeOffset? PublishedAt { get; init; }

        public IReadOnlyCollection<string>? Tags { get; init; }
        public required string Author { get; init; }

        public required QuestionnaireTemplateType Type { get; init; }

        public int? EstimatedCompletionTimeMinutes { get; init; }

    }
}
