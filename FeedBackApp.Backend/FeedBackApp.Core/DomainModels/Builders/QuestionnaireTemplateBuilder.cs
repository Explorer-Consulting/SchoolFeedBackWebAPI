using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.DomainModels.Builders
{
    public sealed class QuestionnaireTemplateBuilder
        : IAggregateBuilder<QuestionnaireTemplate>
    {
        public string StorageID { get; private set; } = default!;
        public string BusinessID { get; private set; } = default!;
        public QuestionnaireTemplateMetadata Metadata { get; private set; } = default!;
        public ICollection<QuestionItem> QuestionItems { get; private set; } = default!;

        public QuestionnaireTemplateBuilder WithStorageID(string storageID)
        {
            StorageID = ULID.NewUlid().ToString();
            return this;
        }
        public QuestionnaireTemplateBuilder WithBusinessID(string businessID)
        {
            BusinessID = businessID;
            return this;
        }

        public QuestionnaireTemplateBuilder WithMetadata(QuestionnaireTemplateMetadata metadata)
        {
            Metadata = metadata;
            return this;
        }

        public QuestionnaireTemplateBuilder ConfigureMetadata(Action<QuestionnaireTemplateMetadata> configure)
        {
            var metadataInstance = new QuestionnaireTemplateMetadata();
            configure(metadataInstance);
            Metadata = metadataInstance;
            return this;
        }

        public QuestionnaireTemplateBuilder AddQuestionItem(Action<QuestionItem> configure)
        {
            var questionItemInstance = new QuestionItem();
            configure(questionItemInstance);
            QuestionItems.Add(questionItemInstance);
            return this;
        }
        public QuestionnaireTemplateBuilder WithQuestionItems(ICollection<QuestionItem> questionItems)
        {
            QuestionItems = questionItems;
            return this;
        }

        public QuestionnaireTemplate Build()
        {
            if(StorageID is null) throw new InvalidOperationException("StorageID must be provided");
            if (BusinessID is null) throw new InvalidOperationException("BusinessID must be provided");
            if (Metadata is null) throw new InvalidOperationException("Metadata must be provided");
            if (QuestionItems is null || QuestionItems.Count == 0) throw new InvalidOperationException("QuestionItems must be provided
            return new QuestionnaireTemplate
            {
                QuestionnaireTemplateStorageID = StorageID,
                QuestionnaireTemplateBusinessID = BusinessID,
                Metadata = Metadata,
                QuestionItems = QuestionItems
            };
        }

    }
}
