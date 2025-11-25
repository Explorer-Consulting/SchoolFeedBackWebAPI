using Core.DomainModels;
using Core.DomainModels.Builders;

namespace Core.Interfaces;

/// <summary>
/// Defines a repository interface for managing aggregate roots of questionnaire templates.
/// </summary>
/// <remarks>This interface provides methods for accessing and persisting questionnaire template aggregates using
/// the builder pattern. It is typically used to abstract data storage and retrieval operations for questionnaire
/// templates within a domain-driven design context.</remarks>
public interface IQuestionnaireTemplateAggregateRepository
    : IAggregateRepository<QuestionnaireTemplate, QuestionnaireTemplateBuilder>
{
}
