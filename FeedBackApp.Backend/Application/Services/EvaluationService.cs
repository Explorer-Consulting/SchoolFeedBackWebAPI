using Application.DTOs.Evaluation;
using Application.Extensions.EvaluationExtensions;
using Application.Services.Interfaces;
using Application.Validation.SubmitValidation;
using Application.Validation.UpdateValidation;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using FluentValidation;

namespace Application.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IEvaluationRepository _repository;

        public EvaluationService(IEvaluationRepository repository)
        {
            _repository = repository;
        }

        public Task<UpdateResponseDTO> UpdateQuestionnaire(string id, UpdateQuestionnaireDTO dto)
        {
            return HandleQuestionnaireAsync(
                id,
                dto,
                templates => new UpdateQuestionnaireValidator(templates),
                (newQ, oldQ) => _repository.UpdateQuestionnaire(newQ, oldQ),
                (success, qid, errors) => success
                    ? new UpdateResponseDTO(true, $"Questionnaire {qid} was updated successfully.")
                    : new UpdateResponseDTO(false, errors ?? $"Update questionnaire {qid} failed"),
                d => d.ToModel()
            );
        }


        public Task<SubmitResponseDTO> SubmitQuestionnaire(string id, SubmitQuestionnaireDTO dto)
        {
            return HandleQuestionnaireAsync(
                id,
                dto,
                templates => new SubmitQuestionnaireValidator(templates),
                async (newQ, oldQ) =>
                {
                    oldQ.Status = true;
                    return await _repository.SubmitQuestionnaire(newQ, oldQ);
                },
                (success, qid, errors) => success
                    ? new SubmitResponseDTO(true, $"Questionnaire {qid} was submitted successfully.")
                    : new SubmitResponseDTO(false, errors ?? $"Submit questionnaire {qid} failed"),
                d => d.ToModel()
            );
        }


        private async Task<TResponse> HandleQuestionnaireAsync<TDto, TResponse>(
            string id,
            TDto dto,
            Func<IList<QuestionTemplate>, IValidator<TDto>> validatorFactory,
            Func<Questionnaire, Questionnaire, Task<bool>> repoAction,
            Func<bool, string, string?, TResponse> responseFactory,
            Func<TDto, Questionnaire> mapToModel
        )
        where TDto : class
        {
            var oldQuestionnaire = await _repository.GetQuestionnaireByIdAsync(id);
            if (oldQuestionnaire == null)
                return responseFactory(false, id, $"Questionnaire {id} not found.");

            var questionTemplate = await _repository.GetQuestionTemplateBySurveyIdAsync(oldQuestionnaire.SurveyId);
            if (questionTemplate == null)
            {
                return responseFactory(false, id, $"QuestionnaireTemplates {id} not found.");
            }

            var endDate = await _repository.GetEndDateBySurveyId(oldQuestionnaire.SurveyId);

            if(endDate < DateTime.UtcNow)
            {
                return responseFactory(false, id, endDate.ToString());
            }

            var validator = validatorFactory(questionTemplate.QuestionTemplates);
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return responseFactory(false, id, $"Validation failed: {errors}");
            }

            var model = mapToModel(dto);

            bool success = await repoAction(model, oldQuestionnaire);

            return responseFactory(success, id, null);
        }
    }
}
