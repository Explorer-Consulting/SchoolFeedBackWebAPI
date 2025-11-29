using ApplicationLayer.DataTransferObjects;
using Core.DomainModels;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Mappings
{
    public sealed class QuestionnaireTemplateMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<QuestionnaireTemplate, QuestionnaireTemplateDTO>();
            config.NewConfig<QuestionnaireTemplateDTO, QuestionnaireTemplate>();
        }
    }
}
