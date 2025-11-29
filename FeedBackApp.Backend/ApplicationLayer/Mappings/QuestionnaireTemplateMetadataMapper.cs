using ApplicationLayer.DataTransferObjects;
using Core.DomainModels;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Mappings
{
    public sealed class QuestionnaireTemplateMetadataMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<QuestionnaireTemplateMetadata, QuestionnaireTemplateMetadataDTO>();
            config.NewConfig<QuestionnaireTemplateMetadataDTO, QuestionnaireTemplateMetadata>();
        }
    }
}
