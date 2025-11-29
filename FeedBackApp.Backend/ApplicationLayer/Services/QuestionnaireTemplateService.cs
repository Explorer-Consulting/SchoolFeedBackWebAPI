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
using System.Linq.Expressions;

namespace ApplicationLayer.Services;

public sealed class QuestionnaireTemplateService(
    ILogger<QuestionnaireTemplateService> logger,
    IQuestionnaireTemplateAggregateRepository aggregateRepository,
    IValidator<QuestionnaireTemplateDTO> validator
) : IAggregateService<QuestionnaireTemplateDTO, QuestionnaireTemplate>
{
    public async Task<Result> ConstructAggreateInstanceAsync(QuestionnaireTemplateDTO dto)
    {
        logger.LogInformation(
            "Creating QuestionnaireTemplate. Incoming DTO: {@DTO}",
            dto);

        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            logger.LogWarning(
                "Validation failed for QuestionnaireTemplate create. Errors: {@Errors}",
                validation.Errors);

            var result = Result.Fail("Validation failed for QuestionnaireTemplate.");
            foreach (var error in validation.Errors)
            {
                result.WithError(error.ErrorMessage);
            }

            return result;
        }

        try
        {
            var aggregate = dto.Adapt<QuestionnaireTemplate>();

            logger.LogInformation(
                "Persisting new QuestionnaireTemplate. BusinessID={BusinessID}",
                aggregate.QuestionnaireTemplateBusinessID);

            await aggregateRepository.ConstructAggregateInstanceAsync(aggregate);

            logger.LogInformation(
                "QuestionnaireTemplate created. BusinessID={BusinessID}",
                aggregate.QuestionnaireTemplateBusinessID);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while creating QuestionnaireTemplate. DTO={@DTO}",
                dto);

            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result> DeleteAggregateAsync(string aggregateId)
    {
        if (string.IsNullOrWhiteSpace(aggregateId))
            return Result.Fail("AggregateId is required.");

        logger.LogInformation(
            "Deleting QuestionnaireTemplate. BusinessID={BusinessID}",
            aggregateId);

        try
        {
            await aggregateRepository.DeleteAggregateAsync(aggregateId);

            logger.LogInformation(
                "QuestionnaireTemplate deletion processed. BusinessID={BusinessID}",
                aggregateId);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while deleting QuestionnaireTemplate. BusinessID={BusinessID}",
                aggregateId);

            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result> UpdateAggregateAsync(QuestionnaireTemplateDTO dto)
    {
        logger.LogInformation(
            "Updating QuestionnaireTemplate. DTO BusinessID={BusinessID}, DTO={@DTO}",
            dto.QuestionnaireTemplateBusinessID,
            dto);

        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            logger.LogWarning(
                "Validation failed for QuestionnaireTemplate update. Errors: {@Errors}",
                validation.Errors);

            var result = Result.Fail("Validation failed for QuestionnaireTemplate.");
            foreach (var error in validation.Errors)
            {
                result.WithError(error.ErrorMessage);
            }

            return result;
        }

        try
        {
            var aggregate = dto.Adapt<QuestionnaireTemplate>();

            logger.LogInformation(
                "Persisting update for QuestionnaireTemplate {BusinessID}",
                aggregate.QuestionnaireTemplateBusinessID);

            await aggregateRepository.UpdateAggregateAsync(aggregate);

            logger.LogInformation(
                "QuestionnaireTemplate {BusinessID} updated.",
                aggregate.QuestionnaireTemplateBusinessID);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while updating QuestionnaireTemplate. DTO={@DTO}",
                dto);

            return Result.Fail(new ExceptionalError(ex));
        }
    }

    public async Task<Result<QuestionnaireTemplateDTO>> RetrieveAggregateAsync(
        Expression<Func<QuestionnaireTemplate, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        logger.LogInformation(
            "Retrieving QuestionnaireTemplate with predicate {Predicate}",
            predicate);

        try
        {
            var aggregate = await aggregateRepository.RetrieveAggregateAsync(predicate);

            if (aggregate is null)
            {
                logger.LogWarning(
                    "No QuestionnaireTemplate found for predicate {Predicate}",
                    predicate);

                return Result.Fail<QuestionnaireTemplateDTO>("QuestionnaireTemplate not found.");
            }

            var dto = aggregate.Adapt<QuestionnaireTemplateDTO>();

            logger.LogInformation(
                "Retrieved QuestionnaireTemplate. BusinessID={BusinessID}",
                aggregate.QuestionnaireTemplateBusinessID);

            return Result.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while retrieving QuestionnaireTemplate with predicate {Predicate}",
                predicate);

            return Result.Fail<QuestionnaireTemplateDTO>(new ExceptionalError(ex));
        }
    }

    public async Task<Result<IReadOnlyCollection<QuestionnaireTemplateDTO>>> RetrieveAllAggregatesAsync(Expression<Func<QuestionnaireTemplate, bool>>? predicate = null)
    {
        logger.LogInformation(
            "Retrieving all QuestionnaireTemplates. HasPredicate={HasPredicate}",
            predicate is not null);

        try
        {
            var list = new List<QuestionnaireTemplateDTO>();

            await foreach (var aggregate in aggregateRepository.RetrieveAllAggregatesAsync(predicate))
            {
                var dto = aggregate.Adapt<QuestionnaireTemplateDTO>();

                logger.LogDebug(
                    "Mapping QuestionnaireTemplate to DTO. BusinessID={BusinessID}",
                    dto.QuestionnaireTemplateBusinessID);

                list.Add(dto);
            }

            logger.LogInformation(
                "Finished retrieving QuestionnaireTemplates. Count={Count}",
                list.Count);

            IReadOnlyCollection<QuestionnaireTemplateDTO> readOnly = list;

            return Result.Ok(readOnly);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while retrieving all QuestionnaireTemplates.");

            return Result.Fail<IReadOnlyCollection<QuestionnaireTemplateDTO>>(
                new ExceptionalError(ex));
        }
    }


}
