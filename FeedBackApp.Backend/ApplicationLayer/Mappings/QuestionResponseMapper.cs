using ApplicationLayer.DataTransferObjects;
using Core.DomainModels;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Mappings
{
    public sealed class QuestionResponseMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<QuestionResponse, QuestionResponseDTO>();
            config.NewConfig<QuestionResponseDTO, QuestionResponse>();
        }
    }
}
