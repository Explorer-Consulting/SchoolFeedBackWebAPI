using Core.DomainModels;
using Core.DomainModels.Builders;

namespace Core.Interfaces;

public interface IQuestionnaireResponseRepository
    : IAggregateRepository<QuestionnaireResponse, QuestionnaireResponseBuilder>
{
}
