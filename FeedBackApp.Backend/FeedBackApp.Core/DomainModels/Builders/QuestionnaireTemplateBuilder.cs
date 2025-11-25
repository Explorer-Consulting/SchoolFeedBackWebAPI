using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.DomainModels.Builders
{
    /// <summary>
    /// Provides a fluent builder for constructing instances of <see cref="QuestionnaireTemplate"/> with configurable
    /// metadata and question items.
    /// </summary>
    /// <remarks>Use this class to incrementally configure the properties of a questionnaire template before
    /// creating the final aggregate. The builder enforces required fields and validates input during the build process.
    /// This class is not thread-safe and should be used from a single thread.</remarks>
    public sealed class QuestionnaireTemplateBuilder
        : IAggregateBuilder<QuestionnaireTemplate>
    {
        public string? StorageId { get; private set; }
        public string? BusinessId { get; private set; }
        public QuestionnaireTemplateMetadata? Metadata { get; private set; }
        public ICollection<QuestionItem> QuestionItems { get; private set; } = [];

        /// <summary>
        /// Sets the storage identifier for the questionnaire template builder.
        /// </summary>
        /// <param name="storageId">The unique identifier to associate with the storage location. Cannot be null, empty, or consist only of
        /// white-space characters.</param>
        /// <returns>The current instance of <see cref="QuestionnaireTemplateBuilder"/> with the specified storage identifier
        /// set.</returns>
        public QuestionnaireTemplateBuilder WithStorageId(string storageId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(storageId);
            StorageId = storageId;
            return this;
        }

        /// <summary>
        /// Generates a new unique storage identifier for the questionnaire template and assigns it to the current
        /// instance.
        /// </summary>
        /// <returns>The current <see cref="QuestionnaireTemplateBuilder"/> instance with the updated storage identifier.</returns>
        public QuestionnaireTemplateBuilder GenerateStorageId()
        {
            StorageId = ULID.NewUlid().ToString();
            return this;
        }

        /// <summary>
        /// Sets the business identifier for the questionnaire template builder.
        /// </summary>
        /// <param name="businessId">The unique identifier of the business to associate with the questionnaire template. Cannot be null, empty,
        /// or consist only of white-space characters.</param>
        /// <returns>The current instance of <see cref="QuestionnaireTemplateBuilder"/> with the specified business identifier
        /// set.</returns>
        public QuestionnaireTemplateBuilder WithBusinessId(string businessId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(businessId);
            BusinessId = businessId;
            return this;
        }

        /// <summary>
        /// Sets the metadata for the questionnaire template and returns the current builder instance to enable fluent
        /// configuration.
        /// </summary>
        /// <param name="metadata">The metadata to associate with the questionnaire template. Cannot be null.</param>
        /// <returns>The current <see cref="QuestionnaireTemplateBuilder"/> instance with the specified metadata applied.</returns>
        public QuestionnaireTemplateBuilder WithMetadata(QuestionnaireTemplateMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(metadata);
            Metadata = metadata;
            return this;
        }

        /// <summary>
        /// Configures the metadata for the questionnaire template using the specified delegate.
        /// </summary>
        /// <remarks>Use this method to set custom metadata values for the questionnaire template before
        /// building or using it. The <paramref name="configure"/> delegate is invoked with a new <see
        /// cref="QuestionnaireTemplateMetadata"/> instance, allowing you to set its properties as needed.</remarks>
        /// <param name="configure">An action delegate that receives a <see cref="QuestionnaireTemplateMetadata"/> instance to configure its
        /// properties.</param>
        /// <returns>The current <see cref="QuestionnaireTemplateBuilder"/> instance with updated metadata.</returns>
        public QuestionnaireTemplateBuilder ConfigureMetadata(Action<QuestionnaireTemplateMetadata> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var instance = new QuestionnaireTemplateMetadata();
            configure(instance);
            Metadata = instance;

            return this;
        }

        /// <summary>
        /// Sets the collection of question items for the questionnaire template builder.
        /// </summary>
        /// <param name="items">The collection of <see cref="QuestionItem"/> objects to associate with the questionnaire template. Cannot be
        /// null.</param>
        /// <returns>The current <see cref="QuestionnaireTemplateBuilder"/> instance with the specified question items applied.</returns>
        public QuestionnaireTemplateBuilder WithQuestionItems(ICollection<QuestionItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            QuestionItems = items;
            return this;
        }

        /// <summary>
        /// Adds a new question item to the questionnaire and allows configuration of its properties.
        /// </summary>
        /// <remarks>Use this method to add and customize individual question items within a questionnaire
        /// template. The <paramref name="configure"/> action is invoked with a new <see cref="QuestionItem"/> instance,
        /// which is then added to the template.</remarks>
        /// <param name="configure">An action delegate used to configure the newly created <see cref="QuestionItem"/> before it is added. Cannot
        /// be null.</param>
        /// <returns>The current <see cref="QuestionnaireTemplateBuilder"/> instance, enabling method chaining.</returns>
        public QuestionnaireTemplateBuilder AddQuestionItem(Action<QuestionItem> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var item = new QuestionItem();
            configure(item);
            QuestionItems.Add(item);

            return this;
        }

        /// <summary>
        /// Asynchronously constructs a new instance of the aggregate questionnaire template using the current property
        /// values.
        /// </summary>
        /// <remarks>The returned aggregate will use the current values of the storage ID, business ID,
        /// metadata, and question items. The storage ID is automatically generated if not set prior to calling this
        /// method.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the constructed <see
        /// cref="QuestionnaireTemplate"/> instance with the specified storage ID, business ID, metadata, and question
        /// items.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no question items have been added to the template.</exception>
        public Task<QuestionnaireTemplate> BuildAggregateAsync()
        {
            StorageId ??= ULID.NewUlid().ToString();

            ArgumentException.ThrowIfNullOrWhiteSpace(BusinessId);
            ArgumentNullException.ThrowIfNull(Metadata);

            if (QuestionItems.Count == 0)
                throw new InvalidOperationException("At least one QuestionItem is required.");

            var agg = new QuestionnaireTemplate
            {
                QuestionnaireTemplateStorageID = StorageId,
                QuestionnaireTemplateBusinessID = BusinessId!,
                Metadata = Metadata!,
                QuestionItems = QuestionItems
            };

            return Task.FromResult(agg);
        }
    }
}
