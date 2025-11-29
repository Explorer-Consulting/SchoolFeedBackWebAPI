using ApplicationLayer.DataTransferObjects;
using ApplicationLayer.Interfaces;
using Core.DomainModels;
using Core.Interfaces;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ApplicationLayer.Services;

public sealed class QuestionnaireResponseService(
    ILogger<QuestionnaireResponseService> logger,
    IQuestionnaireResponseAggregateRepository aggregateRepository,
    IValidator<QuestionnaireResponseDTO> validator
) : IAggregateService<QuestionnaireResponseDTO, QuestionnaireResponse>
{
    public async Task ConstructAggreateInstanceAsync(QuestionnaireResponseDTO dto)
    {
        logger.LogInformation(
            "[Service] Constructing QuestionnaireResponse aggregate. DTO={@DTO}",
            dto);

        await validator.ValidateAndThrowAsync(dto);
        logger.LogDebug("[Service] DTO validation passed for Create.");

        var aggregate = dto.Adapt<QuestionnaireResponse>();
        logger.LogDebug(
            "[Service] Mapped DTO to aggregate. BusinessID={BusinessID}",
            aggregate.QuestionnaireResponseBusinessID);

        await aggregateRepository.ConstructAggregateInstanceAsync(aggregate);

        logger.LogInformation(
            "[Service] QuestionnaireResponse created. BusinessID={BusinessID}",
            aggregate.QuestionnaireResponseBusinessID);
    }

    public async Task DeleteAggregateAsync(string aggregateId)
    {
        logger.LogInformation(
            "[Service] Deleting QuestionnaireResponse. BusinessID={BusinessID}",
            aggregateId);

        await aggregateRepository.DeleteAggregateAsync(aggregateId);

        logger.LogInformation(
            "[Service] QuestionnaireResponse deletion processed. BusinessID={BusinessID}",
            aggregateId);
    }

    public async Task UpdateAggregateAsync(QuestionnaireResponseDTO dto)
    {
        logger.LogInformation(
            "[Service] Updating QuestionnaireResponse. DTO BusinessID={BusinessID}",
            dto.QuestionnaireResponseBusinessID);

        await validator.ValidateAndThrowAsync(dto);
        logger.LogDebug("[Service] DTO validation passed for Update.");

        var aggregate = dto.Adapt<QuestionnaireResponse>();

        logger.LogDebug(
            "[Service] Mapped DTO to aggregate for update. BusinessID={BusinessID}",
            aggregate.QuestionnaireResponseBusinessID);

        await aggregateRepository.UpdateAggregateAsync(aggregate);

        logger.LogInformation(
            "[Service] QuestionnaireResponse updated. BusinessID={BusinessID}",
            aggregate.QuestionnaireResponseBusinessID);
    }

    public async Task<QuestionnaireResponseDTO?> RetrieveAggregateAsync(
        Expression<Func<QuestionnaireResponse, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        logger.LogInformation(
            "[Service] Retrieving single QuestionnaireResponse with predicate {Predicate}",
            predicate);

        var aggregate = await aggregateRepository.RetrieveAggregateAsync(predicate);

        if (aggregate is null)
        {
            logger.LogWarning(
                "[Service] No QuestionnaireResponse found for predicate {Predicate}",
                predicate);
            return null;
        }

        logger.LogDebug(
            "[Service] Mapping QuestionnaireResponse to DTO. BusinessID={BusinessID}",
            aggregate.QuestionnaireResponseBusinessID);

        var dto = aggregate.Adapt<QuestionnaireResponseDTO>();

        logger.LogInformation(
            "[Service] QuestionnaireResponse retrieved. BusinessID={BusinessID}",
            aggregate.QuestionnaireResponseBusinessID);

        return dto;
    }

    public async IAsyncEnumerable<QuestionnaireResponseDTO> RetrieveAllAggregatesAsync(
        Expression<Func<QuestionnaireResponse, bool>>? predicate = null)
    {
        logger.LogInformation(
            "[Service] Retrieving all QuestionnaireResponse aggregates. HasPredicate={HasPredicate}",
            predicate is not null);

        await foreach (var aggregate in aggregateRepository.RetrieveAllAggregatesAsync(predicate))
        {
            logger.LogDebug(
                "[Service] Mapping QuestionnaireResponse to DTO. BusinessID={BusinessID}",
                aggregate.QuestionnaireResponseBusinessID);

            yield return aggregate.Adapt<QuestionnaireResponseDTO>();
        }

        logger.LogInformation(
            "[Service] Completed streaming QuestionnaireResponse DTOs.");
    }
}
