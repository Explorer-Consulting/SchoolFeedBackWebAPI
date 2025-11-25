using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.DomainModels.Builders
{
    public sealed class QuestionnaireResponseBuilder
        : IAggregateBuilder<QuestionnaireResponse>
    {
        public string? StorageId { get; private set; }
        public string? BusinessId { get; private set; }
        public string? TemplateBusinessId { get; private set; }
        public string? AssigneeId { get; private set; }
        public ICollection<string> Tags { get; private set; } = [];
        public ICollection<QuestionResponse> QuestionResponses { get; private set; } = [];
        public ResponseStatus? Status { get; private set; }

        public QuestionnaireResponseBuilder WithStorageId(string storageId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(storageId);
            StorageId = storageId;
            return this;
        }

        public QuestionnaireResponseBuilder GenerateStorageId()
        {
            StorageId = ULID.NewUlid().ToString();
            return this;
        }

        public QuestionnaireResponseBuilder WithBusinessId(string businessId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(businessId);
            BusinessId = businessId;
            return this;
        }

        public QuestionnaireResponseBuilder WithTemplateBusinessId(string templateBusinessId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(templateBusinessId);
            TemplateBusinessId = templateBusinessId;
            return this;
        }

        public QuestionnaireResponseBuilder WithAssigneeId(string assigneeId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assigneeId);
            AssigneeId = assigneeId;
            return this;
        }

        public QuestionnaireResponseBuilder WithTags(ICollection<string> tags)
        {
            ArgumentNullException.ThrowIfNull(tags);
            Tags = [.. tags];
            return this;
        }

        public QuestionnaireResponseBuilder AddTag(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);
            Tags.Add(tag);
            return this;
        }

        public QuestionnaireResponseBuilder WithQuestionResponses(ICollection<QuestionResponse> responses)
        {
            ArgumentNullException.ThrowIfNull(responses);
            QuestionResponses = [.. responses];
            return this;
        }

        public QuestionnaireResponseBuilder AddQuestionResponse(QuestionResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);
            QuestionResponses.Add(response);
            return this;
        }

        public QuestionnaireResponseBuilder WithStatus(ResponseStatus status)
        {
            Status = status;
            return this;
        }

        public Task<QuestionnaireResponse> BuildAggregateAsync()
        {
            StorageId ??= ULID.NewUlid().ToString();

            ArgumentException.ThrowIfNullOrWhiteSpace(BusinessId);
            ArgumentException.ThrowIfNullOrWhiteSpace(TemplateBusinessId);
            ArgumentException.ThrowIfNullOrWhiteSpace(AssigneeId);

            if (Status is null)
                throw new InvalidOperationException("Status must be set.");

            var agg = new QuestionnaireResponse
            {
                QuestionnaireResponseStorageID = StorageId,
                QuestionnaireResponseBusinessID = BusinessId!,
                QuestionnaireTemplateBusinessID = TemplateBusinessId!,
                AssigneeID = AssigneeId!,
                Tags = Tags,
                QuestionResponses = QuestionResponses,
                Status = Status.Value
            };

            return Task.FromResult(agg);
        }
    }
}
