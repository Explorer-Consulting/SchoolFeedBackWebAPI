using Application.DTOs.Questionnaire;
using Application.DTOs.Questionnaire.Post;
using Application.DTOs.Survey;
using Application.Extensions.QuestionnaireExtensions;
using Application.Services.Interfaces;
using FeedBackApp.Core.Repositories;
using FluentValidation;

namespace Application.Services
{
    /// <summary>
    /// Orchestrates survey authoring and student-scoped questionnaire retrieval.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Responsibilities</b><br/>
    /// Validates incoming survey metadata, compiles domain entities, persists survey/questionnaire artifacts,
    /// enforces dependency ordering between questions, maintains the student whitelist, and projects
    /// student-specific questionnaire views for the UI.
    /// </para>
    /// <para>
    /// <b>Validation &amp; invariants</b><br/>
    /// Uses FluentValidation to check <see cref="CreateSurveyMetadataDTO"/>. Additionally enforces that each
    /// question dependency references an earlier question (strict ordering) and that all dependency ids resolve.
    /// </para>
    /// <para>
    /// <b>Whitelist management</b><br/>
    /// During compilation, all student emails found in the survey's student sets are upserted into the whitelist
    /// via <see cref="IWhitelistRepository"/>, ensuring subsequent authentication/authorization aligns with survey scope.
    /// </para>
    /// </remarks>
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IEvaluationRepository _evaluationRepository;
        private readonly IValidator<CreateSurveyMetadataDTO> _createValidator;
        private readonly IWhitelistRepository _whitelistRepository;

        /// <summary>
        /// Constructs the questionnaire service with repositories and validators.
        /// </summary>
        /// <param name="questionnaireRepository">Persistence for survey/questionnaire artifacts.</param>
        /// <param name="evaluationRepository">Access to question templates for rendering questionnaires.</param>
        /// <param name="createValidator">FluentValidation validator for survey creation DTOs.</param>
        /// <param name="whitelistRepository">Repository handling student email whitelist.</param>
        public QuestionnaireService(
            IQuestionnaireRepository questionnaireRepository,
            IEvaluationRepository evaluationRepository,
            IValidator<CreateSurveyMetadataDTO> createValidator,
            IWhitelistRepository whitelistRepository)
        {
            _questionnaireRepository = questionnaireRepository;
            _evaluationRepository = evaluationRepository;
            _createValidator = createValidator;
            _whitelistRepository = whitelistRepository;
        }

        /// <summary>
        /// Validates, compiles and persists a new survey with its metadata and questionnaires.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Flow</b><br/>
        /// 1) Validate <paramref name="dto"/> with FluentValidation.<br/>
        /// 2) Map to domain <see cref="SurveyMetadata"/>.<br/>
        /// 3) Verify that every dependency refers to an earlier question (e.g., <c>q0</c> can be a dependency for <c>q3</c>, but not vice versa).<br/>
        /// 4) Upsert all student emails into the whitelist.<br/>
        /// 5) Persist compiled artifacts via <see cref="IQuestionnaireRepository.CompileAndSaveAsync(SurveyMetadata)"/>.
        /// </para>
        /// <para>
        /// Returns a typed result indicating success or the combined validation/domain error messages.
        /// </para>
        /// </remarks>
        /// <param name="dto">Creation metadata including teachers, templates, student sets, and creation parameters.</param>
        /// <returns>A <see cref="CreationResponseDTO"/> with status and message.</returns>
        public async Task<CreationResponseDTO> CompileAndSaveAsync(CreateSurveyMetadataDTO dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new CreationResponseDTO(false, errors);
            }

            var metadata = dto.ToModel();

            for (int i = 0; i < metadata.QuestionTemplates.Count; i++)
            {
                var current = metadata.QuestionTemplates[i];

                if (current.Dependency == null)
                    continue;

                // Ensure dependency references an earlier question
                int depIndex = -1;
                for (int j = 0; j < metadata.QuestionTemplates.Count; j++)
                {
                    if ($"q{j}" == current.Dependency.Id)
                    {
                        depIndex = j;
                        break;
                    }
                }

                // Maintain whitelist with all student emails involved in the survey
                var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
                foreach (var set in metadata.StudentSets)
                {
                    foreach (var email in set.StudentEmails)
                    {
                        if (!whitelist.StudentEmails.Contains(email))
                        {
                            whitelist.StudentEmails.Add(email);
                        }
                    }
                }
                await _whitelistRepository.UpdateStudentWhitelistAsync(whitelist);

                if (depIndex == -1)
                    return new CreationResponseDTO(false, $"Dependency {current.Dependency.Id} not found for question {current.Id}.");

                if (depIndex >= i)
                    return new CreationResponseDTO(false, $"Dependency {current.Dependency.Id} must refer to an earlier question than {current.Id}.");
            }

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

        /// <summary>
        /// Deletes a survey and all associated artifacts (metadata, questionnaires, templates).
        /// </summary>
        /// <param name="id">Survey identifier.</param>
        /// <returns>
        /// A <see cref="DeletionResponseDTO"/> indicating whether all related resources were removed,
        /// or a not-found/error status if any step failed.
        /// </returns>
        public async Task<DeletionResponseDTO> DeleteSurveyAsync(Guid id)
        {
            try
            {
                bool surveyDeleted = await _questionnaireRepository.DeleteSurveyMetadataAsync(id);
                bool questionnairesDeleted = await _questionnaireRepository.DeleteQuestionnairesBySurveyIdAsync(id);
                bool questionTemplateDeleted = await _questionnaireRepository.DeleteQuestionTemplateBySurveyIdAsync(id);

                if (surveyDeleted && questionnairesDeleted && questionTemplateDeleted)
                {
                    return new DeletionResponseDTO(true, $"Survey {id} and related questionnaires were deleted successfully.");
                }
                else
                {
                    return new DeletionResponseDTO(false, $"Survey {id} not found (no survey metadata or questionnaires).");
                }
            }
            catch (Exception ex)
            {
                return new DeletionResponseDTO(false, $"Error deleting survey {id}: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a student-scoped view of questionnaires for a given survey, including teachers and prefilled answers.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Determines the student's class memberships from survey student sets, groups teachers by subject (creation parameters),
        /// assembles questionnaire identifiers, and, for in-progress questionnaires, merges template questions with any saved answers.
        /// Closed (completed) questionnaires are omitted from the response.
        /// </para>
        /// </remarks>
        /// <param name="surveyId">Target survey identifier.</param>
        /// <param name="studentEmail">The student's email used for scoping and ownership.</param>
        /// <returns>A <see cref="QuestionnairesDTO"/> containing class label and subject→teachers→questions tree.</returns>
        public async Task<QuestionnairesDTO> GetQuestionnairesAsync(Guid surveyId, string studentEmail)
        {
            var surveyMetadata = await _questionnaireRepository.GetSurveyMetadataAsync(surveyId);
            if (surveyMetadata == null)
            {
                return new QuestionnairesDTO();
            }

            // Map teacher email → display name
            Dictionary<string, string> teacherData = new();
            var teacherInfo = surveyMetadata.Teachers;
            foreach (var item in teacherInfo)
            {
                teacherData[item.Email] = item.Name;
            }

            // Resolve classes the student belongs to
            var studentSetIds = surveyMetadata.StudentSets
                .Where(set => set.StudentEmails.Contains(studentEmail))
                .Select(set => set.SetId)
                .ToList();

            if (!studentSetIds.Any())
            {
                return new QuestionnairesDTO();
            }

            var response = new QuestionnairesDTO
            {
                Class = string.Join(", ", studentSetIds),
                Subjects = new List<SubjectDTO>()
            };

            // subject -> list of teacher emails for those sets
            Dictionary<string, List<string>> subjectTeachers = new();

            var creationParams = surveyMetadata.CreationParams
                .Where(par => par.StudentSetIds.Any(setId => studentSetIds.Contains(setId)));

            foreach (var item in creationParams)
            {
                if (!subjectTeachers.ContainsKey(item.SubjectName))
                {
                    subjectTeachers[item.SubjectName] = new List<string>();
                }

                subjectTeachers[item.SubjectName].Add(item.TeacherEmail);
            }

            // Build subject/teacher/question tree, including prefilled answers for in-progress questionnaires
            foreach (var keyValuePair in subjectTeachers)
            {
                var subjectDto = new SubjectDTO
                {
                    Name = keyValuePair.Key,
                    Teachers = new List<TeacherDTO>()
                };

                foreach (var teacherEmail in keyValuePair.Value)
                {
                    var teacherDto = new TeacherDTO
                    {
                        Name = teacherData[teacherEmail]
                    };

                    string questionnaireId = $"{studentEmail}_{teacherEmail}_{subjectDto.Name}_{surveyId}";
                    teacherDto.Id = questionnaireId;

                    var questionnaire = await _questionnaireRepository.GetQuestionnaireByIdAsync(questionnaireId);
                    if (questionnaire != null && questionnaire.Status == false)
                    {
                        var questionnaireTemplate = await _evaluationRepository.GetQuestionTemplateBySurveyIdAsync(questionnaire.SurveyId);
                        var answers = questionnaire.QuestionnaireResults;

                        if (questionnaireTemplate == null || questionnaireTemplate.QuestionTemplates == null)
                        {
                            continue;
                        }

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
                                Dependency = template.Dependency?.ToDto(),
                                Description = template.Description,
                                Category = template.Category
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

        /// <summary>
        /// Marks a questionnaires as validated, indicating the QR code has been scanned
        /// </summary>
        /// <param name="validationToken"></param>
        /// <returns></returns>
        public async Task<ValidationResponseDTO> ValidateQuestionnairesAsync(string validationToken)
        {
            try
            {
                var questionnaires = await _questionnaireRepository.GetQuestionnairesByValidationTokenAsync(validationToken);

                if (questionnaires.Count == 0)
                {
                    return new ValidationResponseDTO(false, "Questionnaire not found.");
                }

                foreach(var questionnaire in questionnaires)
                {
                    questionnaire.IsValidate = true;
                    await _questionnaireRepository.UpdateQuestionnaireAsync(questionnaire);
                }

                return new ValidationResponseDTO(true, "Questionnaire validated successfully.");
            }
            catch (Exception ex)
            {
                return new ValidationResponseDTO(false, $"Error validating questionnaire: {ex.Message}");
        }
        }

        /// <summary>
        /// Generates a single validation token shared across all of a student's already-submitted questionnaires
        /// for a given survey, provided the survey requires validation. The token is meant to be displayed as a
        /// QR code and scanned once to validate every one of the student's submissions for that survey at once.
        /// </summary>
        /// <param name="surveyId">Survey identifier.</param>
        /// <param name="studentEmail">The student's email whose submitted questionnaires should receive the token.</param>
        /// <returns>A response containing the generated token, or a failure message.</returns>
        public async Task<GenerateValidationTokenResponseDTO> GenerateValidationTokenAsync(Guid surveyId, string studentEmail)
        {
            try
            {
                var questionTemplate = await _evaluationRepository.GetQuestionTemplateBySurveyIdAsync(surveyId.ToString());
                if (questionTemplate == null)
                {
                    return new GenerateValidationTokenResponseDTO(false, "Survey not found.");
                }

                if (!questionTemplate.RequireValidation)
                {
                    return new GenerateValidationTokenResponseDTO(false, "This survey does not require validation.");
                }

                var questionnaires = await _questionnaireRepository.GetQuestionnairesForStudentAsync(surveyId, studentEmail);
                if (questionnaires.Count == 0)
                {
                    return new GenerateValidationTokenResponseDTO(false, "No submitted questionnaires found for this student.");
                }

                var token = Guid.NewGuid().ToString();
                foreach (var questionnaire in questionnaires)
                {
                    questionnaire.ValidationToken = token;
                    await _questionnaireRepository.UpdateQuestionnaireAsync(questionnaire);
                }

                return new GenerateValidationTokenResponseDTO(true, "Validation token generated successfully.", token);
            }
            catch (Exception ex)
            {
                return new GenerateValidationTokenResponseDTO(false, $"Error generating validation token: {ex.Message}");
            }
        }
    }
}
