using Core.DomainModels;
using Core.DomainModels.Builders;
using Core.Interfaces;
using Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AggregateRepositories
{
    public sealed class QuestionnaireTemplateAggregateRepository
        : IAggregateRepository<QuestionnaireTemplate, QuestionnaireTemplateBuilder>
    {
        public Task ConstructAggregateInstanceAsync(Action<QuestionnaireTemplateBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var builder = new QuestionnaireTemplateBuilder();
            configure(builder);
            builder.Build();

            return Task.CompletedTask;
        }
    }
}
