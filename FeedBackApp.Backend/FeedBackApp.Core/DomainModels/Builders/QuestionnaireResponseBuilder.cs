using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.DomainModels.Builders
{
    /// <summary>
    /// Provides a builder for constructing instances of <see cref="QuestionnaireResponse"/> using a fluent interface.
    /// </summary>
    /// <remarks>Use this class to incrementally set properties and add responses before creating a <see
    /// cref="QuestionnaireResponse"/> aggregate. The builder enforces required fields and validates input values during
    /// the build process. This class is not thread-safe.</remarks>
    public sealed class QuestionnaireResponseBuilder
        : IAggregateBuilder<QuestionnaireResponse>
    {
        /// <summary>
        /// Gets the unique identifier for the storage resource associated with this instance.
        /// </summary>
        public string? StorageId { get; private set; }
        /// <summary>
        /// Gets the unique identifier associated with the business entity.
        /// </summary>
        public string? BusinessId { get; private set; }
        /// <summary>
        /// Gets the unique identifier associated with the business template.
        /// </summary>
        public string? TemplateBusinessId { get; private set; }
        /// <summary>
        /// Gets the unique identifier of the user assigned to this item, if any.
        /// </summary>
        public string? AssigneeId { get; private set; }

        /// <summary>
        /// Gets the collection of tags associated with the current item.
        /// </summary>
        /// <remarks>The returned collection is read-only and cannot be replaced. Tags can be added to or
        /// removed from the collection, but the property itself cannot be set directly.</remarks>
        public ICollection<string> Tags { get; private set; } = [];

        /// <summary>
        /// Gets the collection of responses associated with the questions for this entity.
        /// </summary>
        public ICollection<QuestionResponse> QuestionResponses { get; private set; } = [];
        /// <summary>
        /// Gets the status of the response, indicating the outcome of the operation.
        /// </summary>
        /// <remarks>The value is <see langword="null"/> if the response status has not been set. Use this
        /// property to determine whether the operation was successful, failed, or is in another defined
        /// state.</remarks>
        public ResponseStatus? Status { get; private set; }

        /// <summary>
        /// Sets the storage identifier for the questionnaire response and returns the updated builder instance.
        /// </summary>
        /// <param name="storageId">The unique identifier to associate with the questionnaire response in storage. Cannot be null, empty, or
        /// consist only of white-space characters.</param>
        /// <returns>The current <see cref="QuestionnaireResponseBuilder"/> instance with the specified storage identifier set.</returns>
        public QuestionnaireResponseBuilder WithStorageId(string storageId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(storageId);
            StorageId = storageId;
            return this;
        }

        /// <summary>
        /// Generates a new unique storage identifier and assigns it to the builder.
        /// </summary>
        /// <remarks>This method uses a ULID to ensure the generated storage identifier is unique. Calling
        /// this method will overwrite any previously set storage identifier.</remarks>
        /// <returns>The current instance of <see cref="QuestionnaireResponseBuilder"/> with the updated storage identifier.</returns>
        public QuestionnaireResponseBuilder GenerateStorageId()
        {
            StorageId = ULID.NewUlid().ToString();
            return this;
        }

        /// <summary>
        /// Sets the business identifier for the questionnaire response builder and returns the updated builder
        /// instance.
        /// </summary>
        /// <param name="businessId">The business identifier to associate with the questionnaire response. Cannot be null, empty, or consist only
        /// of white-space characters.</param>
        /// <returns>The current <see cref="QuestionnaireResponseBuilder"/> instance with the specified business identifier set.</returns>
        public QuestionnaireResponseBuilder WithBusinessId(string businessId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(businessId);
            BusinessId = businessId;
            return this;
        }

        /// <summary>
        /// Sets the template business identifier for the questionnaire response builder.
        /// </summary>
        /// <param name="templateBusinessId">The business identifier to associate with the questionnaire template. Cannot be null, empty, or consist only
        /// of white-space characters.</param>
        /// <returns>The current instance of <see cref="QuestionnaireResponseBuilder"/> with the updated template business
        /// identifier.</returns>
        public QuestionnaireResponseBuilder WithTemplateBusinessId(string templateBusinessId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(templateBusinessId);
            TemplateBusinessId = templateBusinessId;
            return this;
        }

        /// <summary>
        /// Sets the assignee identifier for the questionnaire response builder.
        /// </summary>
        /// <param name="assigneeId">The identifier of the user to assign the questionnaire response to. Cannot be null, empty, or consist only
        /// of white-space characters.</param>
        /// <returns>The current <see cref="QuestionnaireResponseBuilder"/> instance with the updated assignee identifier.</returns>
        public QuestionnaireResponseBuilder WithAssigneeId(string assigneeId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assigneeId);
            AssigneeId = assigneeId;
            return this;
        }

        /// <summary>
        /// Sets the collection of tags to associate with the questionnaire response builder.
        /// </summary>
        /// <param name="tags">A collection of strings representing the tags to assign. Cannot be null.</param>
        /// <returns>The current <see cref="QuestionnaireResponseBuilder"/> instance with the specified tags applied.</returns>
        public QuestionnaireResponseBuilder WithTags(ICollection<string> tags)
        {
            ArgumentNullException.ThrowIfNull(tags);
            Tags = [.. tags];
            return this;
        }

        /// <summary>
        /// Adds a tag to the questionnaire response builder.
        /// </summary>
        /// <param name="tag">The tag to add. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <returns>The current instance of <see cref="QuestionnaireResponseBuilder"/> to allow method chaining.</returns>
        public QuestionnaireResponseBuilder AddTag(string tag)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);
            Tags.Add(tag);
            return this;
        }

        /// <summary>
        /// Sets the collection of question responses for the questionnaire builder.
        /// </summary>
        /// <param name="responses">A collection of <see cref="QuestionResponse"/> objects representing the responses to individual questions.
        /// Cannot be null.</param>
        /// <returns>The current <see cref="QuestionnaireResponseBuilder"/> instance with the specified question responses
        /// applied.</returns>
        public QuestionnaireResponseBuilder WithQuestionResponses(ICollection<QuestionResponse> responses)
        {
            ArgumentNullException.ThrowIfNull(responses);
            QuestionResponses = [.. responses];
            return this;
        }

        /// <summary>
        /// Adds a question response to the questionnaire being built.
        /// </summary>
        /// <param name="response">The question response to add. Cannot be null.</param>
        /// <returns>The current <see cref="QuestionnaireResponseBuilder"/> instance, allowing for method chaining.</returns>
        public QuestionnaireResponseBuilder AddQuestionResponse(QuestionResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);
            QuestionResponses.Add(response);
            return this;
        }

        /// <summary>
        /// Sets the response status for the questionnaire and returns the current builder instance to enable method
        /// chaining.
        /// </summary>
        /// <param name="status">The response status to assign to the questionnaire. Specifies the current state of the response.</param>
        /// <returns>The current <see cref="QuestionnaireResponseBuilder"/> instance with the updated status.</returns>
        public QuestionnaireResponseBuilder WithStatus(ResponseStatus status)
        {
            Status = status;
            return this;
        }

        /// <summary>
        /// Asynchronously builds a new aggregate questionnaire response using the current property values.
        /// </summary>
        /// <remarks>All required properties must be set before calling this method. The returned
        /// aggregate reflects the current state of the object at the time of invocation.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="QuestionnaireResponse"/> instance populated with the current values.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <c>Status</c> is <c>null</c>.</exception>
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
