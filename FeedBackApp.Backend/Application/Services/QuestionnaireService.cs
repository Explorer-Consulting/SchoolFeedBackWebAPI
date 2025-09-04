using Application.DTOs.Questionnaire;
using Application.DTOs.Survey;
using Application.Extensions.QuestionnaireExtensions;
using Application.Services.Interfaces;
using FeedBackApp.Core.Repositories;
using FluentValidation;

namespace Application.Services
{
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IEvaluationRepository _evaluationRepository;
        private readonly IValidator<CreateSurveyMetadataDTO> _createValidator;
        public QuestionnaireService(IQuestionnaireRepository questionnaireRepository, IEvaluationRepository evaluationRepository, IValidator<CreateSurveyMetadataDTO> createValidator)
        {
            _questionnaireRepository = questionnaireRepository;
            _evaluationRepository = evaluationRepository;
            _createValidator = createValidator;
        }

        public async Task<CreationResponseDTO> CompileAndSaveAsync(CreateSurveyMetadataDTO dto)
        {

            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new CreationResponseDTO(false, errors);
            }
            var metadata = dto.ToModel();
            try
            {
                await _questionnaireRepository.CompileAndSaveAsync(metadata);

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
                bool surveyDeleted = await _questionnaireRepository.DeleteSurveyMetadataAsync(id);
                bool questionnairesDeleted = await _questionnaireRepository.DeleteQuestionnairesBySurveyIdAsync(id);
                bool questionTemplateDeleted = await _questionnaireRepository.DeleteQuestionTemplateBySurveyIdAsync(id);

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
        public async Task<QuestionnairesDTO> GetQuestionnairesAsync(Guid surveyId, string studentEmail)
        {
            var surveyMetadata = await _questionnaireRepository.GetSurveyMetadataAsync(surveyId);
            if (surveyMetadata == null)
            {
                return new QuestionnairesDTO();
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
                return new QuestionnairesDTO();
            }

            QuestionnairesDTO response = new QuestionnairesDTO();
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

                    var questionnaire = await _questionnaireRepository.GetQuestionnaireByIdAsync(questionnaireId);
                    if (questionnaire != null && questionnaire.Status == false)
                    {
                        List<QuestionDTO> questionDTOs = new List<QuestionDTO>();
                        var questionnaireTemplate = await _evaluationRepository.GetQuestionTemplateBySurveyIdAsync(questionnaire.SurveyId);
                        var answers = questionnaire.QuestionnaireResults;

                        var dtoList = new List<QuestionDTO>();

                        foreach (var template in questionnaireTemplate.QuestionTemplates)
                        {
                            string answer = string.Empty;

                            foreach (var ans in answers)
                            {
                                if (ans.QuestionId == template.Id)
                                {
                                    answer = ans.Answer;
                                    break;
                                }
                            }

                            dtoList.Add(new QuestionDTO
                            {
                                QuestionID = template.Id,
                                Question = template.Question,
                                Type = template.Type,
                                AnswerOptions = template.AnswerOptions,
                                Answer = answer,
                                Dependency = template.Dependency?.ToDto()
                            });
                            
                        }
                        teacherDto.Questions = dtoList;
                        subjectDto.Teachers.Add(teacherDto);
                    }   
                }
                response.Subjects.Add(subjectDto);
            }
            return response;
        }
    }
}
