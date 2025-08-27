using Application.DTOs.Questionnaire;
using Application.Extensions.QuestionnaireExtensions;
using Application.Services.Interfaces;
using FeedBackApp.Core.Repositories;
using FluentValidation;

namespace Application.Services
{
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly IQuestionnaireRepository _repository;
        private readonly IValidator<CreateSurveyMetadataDTO> _createValidator;
        public QuestionnaireService(IQuestionnaireRepository repository, IValidator<CreateSurveyMetadataDTO> createValidator)
        {
            _repository = repository;
            _createValidator = createValidator;
        }

        public async Task<CreationResponseDTO> CompileAndSaveAsync(CreateSurveyMetadataDTO dto)
        {

            var validationResult = await _createValidator.ValidateAsync(dto);
            if(!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new CreationResponseDTO(false, errors);
            }
            var metadata = dto.ToModel();
            try
            {
                await _repository.CompileAndSaveAsync(metadata);
            
                return new CreationResponseDTO(true, "Creation successful!");
            }
            catch (Exception e)
            {
                return new CreationResponseDTO(false, $"Creation failed: {e.Message}");
            }
            
        }

        public async Task<DeletionResponseDTO> DeleteSurveyAsync(Guid id)
        {
            try
            {
                bool surveyDeleted = await _repository.DeleteSurveyMetadataAsync(id);
                bool questionnairesDeleted = await _repository.DeleteQuestionnairesBySurveyIdAsync(id);
                bool questionTemplateDeleted = await _repository.DeleteQuestionTemplateBySurveyIdAsync(id);

                if (surveyDeleted && questionnairesDeleted && questionTemplateDeleted)
                {
                    return new DeletionResponseDTO
                    (
                        true,
                       $"Survey {id} and related questionnaires were deleted successfully."
                    );
                }
                else
                {
                    return new DeletionResponseDTO
                    (
                        false,
                        $"Survey {id} not found (no survey metadata or questionnaires)."
                    );
                }
            }
            catch (Exception ex)
            {
                return new DeletionResponseDTO
                (
                    false,
                    $"Error deleting survey {id}: {ex.Message}"
                );
            }
        }
    }
}
