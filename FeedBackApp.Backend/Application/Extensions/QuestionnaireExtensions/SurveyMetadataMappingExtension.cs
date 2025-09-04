using Application.DTOs.Questionnaire;
using Application.DTOs.Questionnaire.Post;
using Application.DTOs.Survey;
using FeedBackApp.Core.Model;

namespace Application.Extensions.QuestionnaireExtensions
{
    public static class SurveyMetadataMappingExtension
    {
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

        public static StudentSet ToModel(this StudentSetDTO dto) =>
            new()
            {
                SetId = dto.SetId,
                StudentEmails = dto.StudentEmails,
            };

        public static StudentSetDTO ToDto(this StudentSet model) =>
            new()
            {
                SetId = model.SetId,
                StudentEmails = [.. model.StudentEmails]
            };

        public static MetaTeacher ToModel(this MetaTeacherDTO dto) =>
            new()
            {
                Email = dto.Email,
                Name = dto.Name
            };
        public static MetaTeacherDTO ToDto(this MetaTeacher model) =>
            new()
            {
                Email = model.Email,
                Name = model.Name
            };

        public static QuestionnaireCreationParam ToModel(this QuestionnaireCreationParamDTO dto) =>
            new()
            {
                TeacherEmail = dto.TeacherEmail,
                SubjectName = dto.SubjectName,
                StudentSetIds = dto.StudentSetIds
            };

        public static QuestionnaireCreationParamDTO ToDto(this QuestionnaireCreationParam model) =>
            new()
            {
                TeacherEmail = model.TeacherEmail,
                SubjectName = model.SubjectName,
                StudentSetIds = [.. model.StudentSetIds]
            };

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
        public static PostAnswerDto ToDto(this QuestionAnswer model) =>
            new()
            {
                Answer = model.Answer

            };
        public static QuestionAnswer ToModel(this PostAnswerDto dto) =>
            new()
            {
                Answer = dto.Answer,
            };

        public static QuestionTemplate ToModel(this QuestionTemplateDTO dto) =>
            new()
            {
                Question = dto.Question,
                Type = dto.Type,
                AnswerOptions = dto.AnswerOptions,
                Dependency = dto.Dependency?.ToModel(),
                Category = dto.Category,
                Description = dto.Description
            };

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

        public static GetSurveyMetadataDTO ToGetDto(this SurveyMetadata model) =>
            new()
            {
                Id = model.Id,
                Title = model.Title,
                endDate = model.EndDate,
            };
        public static QuestionDependency ToModel(this DependencyDTO dto) =>
            new()
            {
                Id = dto.Id,
                AnswerConditions = dto.AnswerConditions
            };
        public static DependencyDTO ToDto(this QuestionDependency model) =>
            new()
            {
                Id = model.Id,
                AnswerConditions = model.AnswerConditions
            };
    };
}
