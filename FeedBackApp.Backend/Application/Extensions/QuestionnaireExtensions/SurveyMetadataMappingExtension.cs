using Application.DTOs.Questionnaire;
using Application.DTOs.Questionnaire.Post;
using Application.DTOs.Survey;
using FeedBackApp.Core.Model;

namespace Application.Extensions.QuestionnaireExtensions
{
    /// <summary>
    /// Mapping extensions that convert survey/questionnaire DTOs to domain models and back.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Centralizes one-way and two-way transformations between transport-level DTOs
    /// (<see cref="CreateSurveyMetadataDTO"/>, <see cref="QuestionnaireDTO"/>, <see cref="QuestionTemplateDTO"/>,
    /// <see cref="MetaTeacherDTO"/>, <see cref="StudentSetDTO"/>, <see cref="QuestionnaireCreationParamDTO"/>,
    /// <see cref="PostAnswerDto"/>, <see cref="DependencyDTO"/>)
    /// and domain entities (<see cref="SurveyMetadata"/>, <see cref="Questionnaire"/>, <see cref="QuestionTemplate"/>,
    /// <see cref="MetaTeacher"/>, <see cref="StudentSet"/>, <see cref="QuestionnaireCreationParam"/>,
    /// <see cref="QuestionAnswer"/>, <see cref="QuestionDependency"/>).
    /// </para>
    /// <para>
    /// Invariants and domain validation are expected to be handled by validators/services prior to mapping.
    /// Null collections are mapped to empty lists to provide stable, non-null aggregates in the domain layer.
    /// </para>
    /// </remarks>
    public static class SurveyMetadataMappingExtension
    {
        /// <summary>
        /// Maps a creation DTO to a new <see cref="SurveyMetadata"/> with generated identifier.
        /// </summary>
        /// <param name="dto">Creation metadata provided by the caller.</param>
        /// <returns>Initialized <see cref="SurveyMetadata"/> ready for persistence.</returns>
        public static SurveyMetadata ToModel(this CreateSurveyMetadataDTO dto) =>
            new()
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                StudentSets = dto.StudentSets
                    .Select(s => s.ToModel())
                    .ToList() ?? new List<StudentSet>(),
                QuestionTemplates = dto.QuestionTemplates
                    .Select(q => q.ToModel())
                    .ToList() ?? new List<QuestionTemplate>(),
                Teachers = dto.Teachers
                    .Select(t => t.ToModel())
                    .ToList() ?? new List<MetaTeacher>(),
                CreationParams = dto.CreationParams
                    .Select(c => c.ToModel())
                    .ToList() ?? new List<QuestionnaireCreationParam>()
            };

        /// <summary>
        /// Projects a <see cref="SurveyMetadata"/> entity to a <see cref="CreateSurveyMetadataDTO"/>.
        /// </summary>
        /// <param name="model">Domain model instance.</param>
        /// <returns>DTO carrying a serializable view of the survey metadata.</returns>
        public static CreateSurveyMetadataDTO ToDto(this SurveyMetadata model) =>
            new()
            {
                Title = model.Title,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                StudentSets = model.StudentSets
                    .Select(s => s.ToDto())
                    .ToList() ?? new List<StudentSetDTO>(),
                QuestionTemplates = model.QuestionTemplates
                    .Select(q => q.ToDto())
                    .ToList() ?? new List<QuestionTemplateDTO>(),
                Teachers = model.Teachers
                    .Select(t => t.ToDto())
                    .ToList() ?? new List<MetaTeacherDTO>(),
                CreationParams = model.CreationParams
                    .Select(c => c.ToDto())
                    .ToList() ?? new List<QuestionnaireCreationParamDTO>()
            };

        /// <summary>
        /// Maps a student set DTO to its domain counterpart.
        /// </summary>
        /// <param name="dto">Student set DTO.</param>
        /// <returns>Domain <see cref="StudentSet"/>.</returns>
        public static StudentSet ToModel(this StudentSetDTO dto) =>
            new()
            {
                SetId = dto.SetId,
                StudentEmails = dto.StudentEmails,
            };

        /// <summary>
        /// Projects a domain student set to DTO form.
        /// </summary>
        /// <param name="model">Domain <see cref="StudentSet"/>.</param>
        /// <returns><see cref="StudentSetDTO"/> projection.</returns>
        public static StudentSetDTO ToDto(this StudentSet model) =>
            new()
            {
                SetId = model.SetId,
                StudentEmails = [.. model.StudentEmails]
            };

        /// <summary>
        /// Maps a teacher metadata DTO to domain.
        /// </summary>
        /// <param name="dto">Teacher metadata DTO.</param>
        /// <returns>Domain <see cref="MetaTeacher"/>.</returns>
        public static MetaTeacher ToModel(this MetaTeacherDTO dto) =>
            new()
            {
                Email = dto.Email,
                Name = dto.Name
            };

        /// <summary>
        /// Projects a domain teacher metadata to DTO form.
        /// </summary>
        /// <param name="model">Domain <see cref="MetaTeacher"/>.</param>
        /// <returns><see cref="MetaTeacherDTO"/> projection.</returns>
        public static MetaTeacherDTO ToDto(this MetaTeacher model) =>
            new()
            {
                Email = model.Email,
                Name = model.Name
            };

        /// <summary>
        /// Maps questionnaire creation parameters DTO to domain entity.
        /// </summary>
        /// <param name="dto">Questionnaire creation parameters.</param>
        /// <returns>Domain <see cref="QuestionnaireCreationParam"/>.</returns>
        public static QuestionnaireCreationParam ToModel(this QuestionnaireCreationParamDTO dto) =>
            new()
            {
                TeacherEmail = dto.TeacherEmail,
                SubjectName = dto.SubjectName,
                StudentSetIds = dto.StudentSetIds
            };

        /// <summary>
        /// Projects domain questionnaire creation parameters to DTO.
        /// </summary>
        /// <param name="model">Domain <see cref="QuestionnaireCreationParam"/>.</param>
        /// <returns><see cref="QuestionnaireCreationParamDTO"/> projection.</returns>
        public static QuestionnaireCreationParamDTO ToDto(this QuestionnaireCreationParam model) =>
            new()
            {
                TeacherEmail = model.TeacherEmail,
                SubjectName = model.SubjectName,
                StudentSetIds = [.. model.StudentSetIds]
            };

        /// <summary>
        /// Maps a questionnaire DTO to a domain <see cref="Questionnaire"/> entity.
        /// </summary>
        /// <param name="dto">Questionnaire DTO with answers.</param>
        /// <returns>Domain questionnaire with answers; <c>Status</c> is initialized to <c>false</c>.</returns>
        public static Questionnaire ToModel(this QuestionnaireDTO dto) =>
            new()
            {
                SurveyId = dto.SurveyId,
                Status = false,
                TeacherEmail = dto.TeacherEmail,
                StudentEmail = dto.StudentEmail,
                SubjectName = dto.SubjectName,
                QuestionnaireResults = dto.QuestionnaireResults
                    .Select(q => q.ToModel())
                    .ToList() ?? new List<QuestionAnswer>(),
            };

        /// <summary>
        /// Projects a domain <see cref="Questionnaire"/> to a DTO suitable for transport.
        /// </summary>
        /// <param name="model">Domain questionnaire.</param>
        /// <returns><see cref="QuestionnaireDTO"/> projection of scalar fields and answers.</returns>
        public static QuestionnaireDTO ToDto(this Questionnaire model) =>
            new()
            {
                SurveyId = model.SurveyId,
                TeacherEmail = model.TeacherEmail,
                StudentEmail = model.StudentEmail,
                SubjectName = model.SubjectName,
                QuestionnaireResults = model.QuestionnaireResults
                    .Select(q => q.ToDto())
                    .ToList() ?? new List<PostAnswerDto>()
            };

        /// <summary>
        /// Projects a domain <see cref="QuestionAnswer"/> to a postable answer DTO.
        /// </summary>
        /// <param name="model">Domain answer.</param>
        /// <returns><see cref="PostAnswerDto"/> with the answer payload.</returns>
        public static PostAnswerDto ToDto(this QuestionAnswer model) =>
            new()
            {
                Answer = model.Answer
            };

        /// <summary>
        /// Maps a postable answer DTO to a domain <see cref="QuestionAnswer"/>.
        /// </summary>
        /// <param name="dto">Answer DTO.</param>
        /// <returns>Domain answer entity.</returns>
        public static QuestionAnswer ToModel(this PostAnswerDto dto) =>
            new()
            {
                Answer = dto.Answer,
            };

        /// <summary>
        /// Maps a question template DTO to a domain template entity, including optional dependency.
        /// </summary>
        /// <param name="dto">Question template DTO.</param>
        /// <returns>Domain <see cref="QuestionTemplate"/>.</returns>
        public static QuestionTemplate ToModel(this QuestionTemplateDTO dto) =>
            new()
            {
                Question = dto.Question,
                Type = dto.Type,
                AnswerOptions = dto.AnswerOptions,
                Dependency = dto.Dependency?.ToModel(),
                Category = dto.Category,
                Description = dto.Description,
                RequiredValidation = dto.RequiredValidation
            };

        /// <summary>
        /// Projects a domain question template back to DTO form.
        /// </summary>
        /// <param name="model">Domain <see cref="QuestionTemplate"/>.</param>
        /// <returns><see cref="QuestionTemplateDTO"/> projection including optional dependency.</returns>
        public static QuestionTemplateDTO ToDto(this QuestionTemplate model) =>
            new()
            {
                Question = model.Question,
                Type = model.Type,
                AnswerOptions = [.. model.AnswerOptions],
                Dependency = model.Dependency?.ToDto(),
                Category = model.Category,
                Description = model.Description
            };

        /// <summary>
        /// Projects a domain <see cref="SurveyMetadata"/> to a lightweight DTO used for listings.
        /// </summary>
        /// <param name="model">Domain survey metadata.</param>
        /// <returns><see cref="GetSurveyMetadataDTO"/> containing key fields.</returns>
        public static GetSurveyMetadataDTO ToGetDto(this SurveyMetadata model) =>
            new()
            {
                Id = model.Id,
                Title = model.Title,
                endDate = model.EndDate,
            };

        /// <summary>
        /// Maps a dependency DTO to a domain question dependency entity.
        /// </summary>
        /// <param name="dto">Dependency DTO (identifier and answer conditions).</param>
        /// <returns>Domain <see cref="QuestionDependency"/>.</returns>
        public static QuestionDependency ToModel(this DependencyDTO dto) =>
            new()
            {
                Id = dto.Id,
                AnswerConditions = dto.AnswerConditions
            };

        /// <summary>
        /// Projects a domain question dependency to DTO form.
        /// </summary>
        /// <param name="model">Domain <see cref="QuestionDependency"/>.</param>
        /// <returns><see cref="DependencyDTO"/> projection.</returns>
        public static DependencyDTO ToDto(this QuestionDependency model) =>
            new()
            {
                Id = model.Id,
                AnswerConditions = model.AnswerConditions
            };
    }
}
