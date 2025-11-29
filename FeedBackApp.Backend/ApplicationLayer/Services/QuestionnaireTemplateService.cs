using ApplicationLayer.DataTransferObjects;
using ApplicationLayer.Interfaces;
using Core.DomainModels;
using Core.Interfaces;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ApplicationLayer.Services;

public sealed class QuestionnaireTemplateService(
    ILogger<QuestionnaireTemplateService> logger,
    IQuestionnaireTemplateAggregateRepository aggregateRepository,
    IValidator<QuestionnaireTemplateDTO> validator
) : IAggregateService<QuestionnaireTemplateDTO, QuestionnaireTemplate>
{
    public async Task ConstructAggreateInstanceAsync(QuestionnaireTemplateDTO dto)
    {
        logger.LogInformation("Creating QuestionnaireTemplate. Incoming DTO: {@DTO}", dto);

        await validator.ValidateAndThrowAsync(dto);

        var aggregate = dto.Adapt<QuestionnaireTemplate>();

        await aggregateRepository.ConstructAggregateInstanceAsync(aggregate);

        logger.LogInformation(
            "QuestionnaireTemplate created. BusinessID={BusinessID}",
            aggregate.QuestionnaireTemplateBusinessID);
    }

    public Task DeleteAggregateAsync(string aggregateId)
    {
        logger.LogInformation(
            "Deleting QuestionnaireTemplate {BusinessID}",
            aggregateId);

        return aggregateRepository.DeleteAggregateAsync(aggregateId);
    }

    public async Task UpdateAggregateAsync(QuestionnaireTemplateDTO dto)
    {
        logger.LogInformation(
            "Updating QuestionnaireTemplate. DTO BusinessID={BusinessID}, DTO={@DTO}",
            dto.QuestionnaireTemplateBusinessID,
            dto);

        await validator.ValidateAndThrowAsync(dto);

        var aggregate = dto.Adapt<QuestionnaireTemplate>();

        await aggregateRepository.UpdateAggregateAsync(aggregate);

        logger.LogInformation(
            "QuestionnaireTemplate {BusinessID} updated.",
            aggregate.QuestionnaireTemplateBusinessID);
    }

    public async Task<QuestionnaireTemplateDTO?> RetrieveAggregateAsync(
        Expression<Func<QuestionnaireTemplate, bool>> predicate)
    {
        logger.LogInformation(
            "Retrieving QuestionnaireTemplate with predicate {Predicate}",
            predicate);

        var aggregate = await aggregateRepository.RetrieveAggregateAsync(predicate);

        if (aggregate is null)
        {
            logger.LogWarning(
                "No QuestionnaireTemplate found for predicate {Predicate}",
                predicate);

            return null;
        }

        var dto = aggregate.Adapt<QuestionnaireTemplateDTO>();

        logger.LogInformation(
            "Retrieved QuestionnaireTemplate. BusinessID={BusinessID}",
            aggregate.QuestionnaireTemplateBusinessID);

        return dto;
    }

    public async IAsyncEnumerable<QuestionnaireTemplateDTO> RetrieveAllAggregatesAsync(
        Expression<Func<QuestionnaireTemplate, bool>>? predicate = null)
    {
        logger.LogInformation(
            "Retrieving all QuestionnaireTemplates. HasPredicate={HasPredicate}",
            predicate is not null);

        await foreach (var aggregate in aggregateRepository.RetrieveAllAggregatesAsync(predicate))
        {
            var dto = aggregate.Adapt<QuestionnaireTemplateDTO>();

            logger.LogDebug(
                "Streaming QuestionnaireTemplate DTO. BusinessID={BusinessID}",
                dto.QuestionnaireTemplateBusinessID);

            yield return dto;
        }

        logger.LogInformation("Finished streaming QuestionnaireTemplate DTOs.");
    }
}
