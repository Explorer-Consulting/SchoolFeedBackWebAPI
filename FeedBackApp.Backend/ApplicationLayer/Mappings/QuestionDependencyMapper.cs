using ApplicationLayer.DataTransferObjects;
using Core.DomainModels;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Mappings
{
    public sealed class QuestionDependencyMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<QuestionDependency, QuestionDependencyDTO>();
            config.NewConfig<QuestionDependencyDTO, QuestionDependency>();
        }
    }
}
