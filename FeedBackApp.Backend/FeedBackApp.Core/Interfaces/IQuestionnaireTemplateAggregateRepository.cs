using Core.DomainModels;
using Core.DomainModels.Builders;

namespace Core.Interfaces;

public interface IQuestionnaireTemplateAggregateRepository
    : IAggregateRepository<QuestionnaireTemplate, QuestionnaireTemplateBuilder>
{
}
