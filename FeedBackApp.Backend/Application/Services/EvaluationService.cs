
using Application.DTOs.Evaluation;
using Application.Extensions.EvaluationExtensions;
using Application.Extensions.QuestionnaireExtensions;
using Application.Services.Interfaces;
using Application.Validation.UpdateValidation;
using FeedBackApp.Core.Repositories;

namespace Application.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IEvaluationRepository _repository;

        public EvaluationService(IEvaluationRepository repository)
        {
            _repository = repository;
        }

        public async Task<UpdateResponseDTO> UpdateQuestionnaire(string id, UpdateQuestionnaireDTO dto)
        {
            var oldQuestionnaire = await _repository.GetQuestionnaireByIdAsync(id);
            if (oldQuestionnaire == null)
                return new UpdateResponseDTO(false, $"Questionnaire {id} not found.");

            var questionTemplate = await _repository.GetQuestionTemplateBySurveyIdAsync(oldQuestionnaire.SurveyId);

            var validator = new UpdateQuestionnaireValidator(questionTemplate.QuestionTemplates);
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new UpdateResponseDTO(false, $"Validation failed: {errors}");
            }

            var newQuestionnaire = dto.ToModel();
            bool questionnaireUpdated = await _repository.UpdateQuestionnaire(newQuestionnaire, oldQuestionnaire);

            return questionnaireUpdated
                ? new UpdateResponseDTO(true, $"Questionnaire {id} was updated successfully.")
                : new UpdateResponseDTO(false, $"Update questionnaire {id} failed");
        }
    }
}
