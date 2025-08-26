
using Application.DTOs.GetQuestionnaires;
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
        private readonly IValidator<CreateSurveyMetadataDto> _validator;
        public QuestionnaireService(IQuestionnaireRepository repository, IValidator<CreateSurveyMetadataDto> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<CreationResponseDTO> CompileAndSaveAsync(CreateSurveyMetadataDto dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
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
        public async Task<GetQuestionnairesResponseDTO?> GetQuestionnairesAsync(Guid surveyId, string studentEmail)
        {
            var surveyMetadata = await _repository.GetSurveyMetadataAsync(surveyId);
            if (surveyMetadata == null)
            {
                return null;
            }
            Dictionary<string, string> teacherData = new Dictionary<string, string>();
            var teacherInfo = surveyMetadata.Teachers;
            foreach (var item in teacherInfo)
            {
                teacherData[item.Email] = item.Name;
            }
            var studentSetId = surveyMetadata.StudentSets.FirstOrDefault(set => set.StudentEmails.Contains(studentEmail))?.SetId;
            if (studentSetId == null)
            {
                return null;
            }
            GetQuestionnairesResponseDTO response = new GetQuestionnairesResponseDTO();
            response.Class = studentSetId;
            response.Subjects = new List<SubjectDTO>();
            Dictionary<string, List<string>> subjectTeachers = new Dictionary<string, List<string>>();
            var creationParams = surveyMetadata.CreationParams.Where(par => par.StudentSetIds.Any(setId => setId == studentSetId));
            foreach (var item in creationParams)
            {
                if (!subjectTeachers.ContainsKey(item.SubjectName))
                {
                    subjectTeachers[item.SubjectName] = new List<string>();
                }

                subjectTeachers[item.SubjectName].Add(item.TeacherEmail);

            }

            foreach (var keyValuePair in subjectTeachers)
            {
                var subjectDto = new SubjectDTO();
                string subject = keyValuePair.Key;
                subjectDto.Name = subject;
                subjectDto.Teachers = new List<TeacherDTO>();
                List<string> teachers = keyValuePair.Value;
                foreach (var teacherEmail in teachers)
                {
                    TeacherDTO teacherDto = new();
                    teacherDto.Name = teacherData[teacherEmail];
                    string questionnaireId = $"{studentEmail}_{teacherEmail}_{subject}_{surveyId}";
                    teacherDto.Id = questionnaireId;
                    var questionnaire = await _repository.GetQuestionnaireByIdAsync(questionnaireId);
                    if (questionnaire == null)
                    {
                        return null;
                    }
                    List<AnswerDTO> answersDto = new List<AnswerDTO>();
                    var answers = questionnaire.QuestionnaireResults;
                    foreach (var item in answers)
                    {
                        answersDto.Add(new AnswerDTO { QuestionID = item.QuestionId, Answer = item.Answer });
                    }
                    teacherDto.Answers = answersDto;
                    subjectDto.Teachers.Add(teacherDto);
                }
                response.Subjects.Add(subjectDto);

            }
            return response;
        }
    }
}
