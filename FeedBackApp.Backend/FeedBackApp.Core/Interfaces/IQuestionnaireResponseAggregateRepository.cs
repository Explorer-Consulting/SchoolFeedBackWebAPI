using Core.DomainModels;
using Core.DomainModels.Builders;

namespace Core.Interfaces;

/// <summary>
/// Defines a repository interface for managing aggregate operations on questionnaire response entities.
/// </summary>
/// <remarks>This interface extends the generic aggregate repository for questionnaire responses, enabling
/// storage, retrieval, and manipulation of questionnaire response aggregates. Implementations are expected to provide
/// persistence and querying capabilities for questionnaire response data.</remarks>
public interface IQuestionnaireResponseAggregateRepository
    : IAggregateRepository<QuestionnaireResponse, QuestionnaireResponseBuilder>
{
}
