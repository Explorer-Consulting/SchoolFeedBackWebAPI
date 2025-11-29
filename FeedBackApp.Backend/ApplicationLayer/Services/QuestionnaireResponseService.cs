using ApplicationLayer.DataTransferObjects;
using ApplicationLayer.Interfaces;
using Core.DomainModels;
using Core.Interfaces;
using FluentResults;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ApplicationLayer.Services;

public sealed class QuestionnaireResponseService(
    ILogger<QuestionnaireResponseService> logger,
    IQuestionnaireResponseAggregateRepository aggregateRepository,
    IValidator<QuestionnaireResponseDTO> validator
) : IAggregateService<QuestionnaireResponseDTO, QuestionnaireResponse>
{
    public async Task<Result> ConstructAggreateInstanceAsync(QuestionnaireResponseDTO dto)
    {
        logger.LogInformation(
            "[Service] Constructing QuestionnaireResponse aggregate. DTO={@DTO}",
            dto);

        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            logger.LogWarning(
                "[Service] Validation failed for QuestionnaireResponse create. Errors={@Errors}",
                validation.Errors);

            var errors = validation.Errors
                .Select(e => new Error(e.ErrorMessage)
                    .WithMetadata("PropertyName", e.PropertyName));
            return Result.Fail(errors);
        }

        try
        {
            var aggregate = dto.Adapt<QuestionnaireResponse>();

            logger.LogDebug(
                "[Service] Mapped DTO to aggregate. BusinessID={BusinessID}",
                aggregate.QuestionnaireResponseBusinessID);

            await aggregateRepository.ConstructAggregateInstanceAsync(aggregate);

            logger.LogInformation(
                "[Service] QuestionnaireResponse created. BusinessID={BusinessID}",
                aggregate.QuestionnaireResponseBusinessID);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[Service] Unexpected error while creating QuestionnaireResponse.");

            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result> DeleteAggregateAsync(string aggregateId)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
            return Result.Fail("Aggregate ID is required.");

        logger.LogInformation(
            "[Service] Deleting QuestionnaireResponse. BusinessID={BusinessID}",
            aggregateId);

        try
        {
            await aggregateRepository.DeleteAggregateAsync(aggregateId);

            logger.LogInformation(
                "[Service] QuestionnaireResponse deletion processed. BusinessID={BusinessID}",
                aggregateId);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[Service] Unexpected error while deleting QuestionnaireResponse. BusinessID={BusinessID}",
                aggregateId);

            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result> UpdateAggregateAsync(QuestionnaireResponseDTO dto)
    {
        logger.LogInformation(
            "[Service] Updating QuestionnaireResponse. DTO BusinessID={BusinessID}",
            dto.QuestionnaireResponseBusinessID);

        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            logger.LogWarning(
                "[Service] Validation failed for QuestionnaireResponse update. Errors={@Errors}",
                validation.Errors);

            var errors = validation.Errors
                .Select(e => new Error(e.ErrorMessage)
                    .WithMetadata("PropertyName", e.PropertyName));
            return Result.Fail(errors);
        }

        try
        {
            var aggregate = dto.Adapt<QuestionnaireResponse>();

            logger.LogDebug(
                "[Service] Mapped DTO to aggregate for update. BusinessID={BusinessID}",
                aggregate.QuestionnaireResponseBusinessID);

            await aggregateRepository.UpdateAggregateAsync(aggregate);

            logger.LogInformation(
                "[Service] QuestionnaireResponse updated. BusinessID={BusinessID}",
                aggregate.QuestionnaireResponseBusinessID);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[Service] Unexpected error while updating QuestionnaireResponse. BusinessID={BusinessID}",
                dto.QuestionnaireResponseBusinessID);

            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result<QuestionnaireResponseDTO>> RetrieveAggregateAsync(
        Expression<Func<QuestionnaireResponse, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        logger.LogInformation(
            "[Service] Retrieving single QuestionnaireResponse with predicate {Predicate}",
            predicate);

        try
        {
            var aggregate = await aggregateRepository.RetrieveAggregateAsync(predicate);

            if (aggregate is null)
            {
                logger.LogWarning(
                    "[Service] No QuestionnaireResponse found for predicate {Predicate}",
                    predicate);

                return Result.Fail("QuestionnaireResponse not found.");
            }

            logger.LogDebug(
                "[Service] Mapping QuestionnaireResponse to DTO. BusinessID={BusinessID}",
                aggregate.QuestionnaireResponseBusinessID);

            var dto = aggregate.Adapt<QuestionnaireResponseDTO>();

            logger.LogInformation(
                "[Service] QuestionnaireResponse retrieved. BusinessID={BusinessID}",
                aggregate.QuestionnaireResponseBusinessID);

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[Service] Unexpected error while retrieving QuestionnaireResponse.");

            return Result.Fail<QuestionnaireResponseDTO>(new ExceptionalError(ex));
        }
    }

    public async Task<Result<IReadOnlyCollection<QuestionnaireResponseDTO>>> RetrieveAllAggregatesAsync(
        Expression<Func<QuestionnaireResponse, bool>>? predicate = null)
    {
        logger.LogInformation(
            "[Service] Retrieving all QuestionnaireResponse aggregates. HasPredicate={HasPredicate}",
            predicate is not null);

        try
        {
            var list = new List<QuestionnaireResponseDTO>();

            await foreach (var aggregate in aggregateRepository.RetrieveAllAggregatesAsync(predicate))
            {
                logger.LogDebug(
                    "[Service] Mapping QuestionnaireResponse to DTO. BusinessID={BusinessID}",
                    aggregate.QuestionnaireResponseBusinessID);

                list.Add(aggregate.Adapt<QuestionnaireResponseDTO>());
            }

            logger.LogInformation(
                "[Service] Completed retrieving QuestionnaireResponse DTOs. Count={Count}",
                list.Count);

            return Result.Ok<IReadOnlyCollection<QuestionnaireResponseDTO>>(list);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[Service] Unexpected error while retrieving all QuestionnaireResponses.");

            return Result.Fail<IReadOnlyCollection<QuestionnaireResponseDTO>>(new ExceptionalError(ex));
        }
    }
}
