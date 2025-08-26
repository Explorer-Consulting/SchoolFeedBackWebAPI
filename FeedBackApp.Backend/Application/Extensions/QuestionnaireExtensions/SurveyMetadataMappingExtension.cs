using Application.DTOs.Questionnaire;
using Application.DTOs.Survey;
using FeedBackApp.Core.Model;

namespace Application.Extensions.QuestionnaireExtensions
{
    public static class SurveyMetadataMappingExtension
    {
        public static SurveyMetadata ToModel(this CreateSurveyMetadataDto dto) =>
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
        public static CreateSurveyMetadataDto ToDto(this SurveyMetadata model) =>
            new()
            {
                Title = model.Title,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                StudentSets = model.StudentSets
                    .Select(s => s.ToDto())
                    .ToList() ?? new List<StudentSetDto>(),
                QuestionTemplates = model.QuestionTemplates
                    .Select(q => q.ToDto())
                    .ToList() ?? new List<QuestionTemplateDto>(),
                Teachers = model.Teachers
                    .Select(t => t.ToDto())
                    .ToList() ?? new List<MetaTeacherDto>(),
                CreationParams = model.CreationParams
                    .Select(c => c.ToDto())
                    .ToList() ?? new List<QuestionnaireCreationParamDto>()
            };


        public static StudentSet ToModel(this StudentSetDto dto) =>
            new()
            {
                SetId = dto.SetId,
                StudentEmails = dto.StudentEmails,
            };
        public static StudentSetDto ToDto(this StudentSet model) =>
            new()
            {
                SetId = model.SetId,
                StudentEmails = [.. model.StudentEmails]
            };
        public static MetaTeacher ToModel(this MetaTeacherDto dto) =>
            new()
            {
                Email = dto.Email,
                Name = dto.Name
            };
        public static MetaTeacherDto ToDto(this MetaTeacher model) =>
            new()
            {
                Email = model.Email,
                Name = model.Name
            };

        public static QuestionnaireCreationParam ToModel(this QuestionnaireCreationParamDto dto) =>
            new()
            {
                TeacherEmail = dto.TeacherEmail,
                SubjectName = dto.SubjectName,
                StudentSetIds = dto.StudentSetIds
            };
        public static QuestionnaireCreationParamDto ToDto(this QuestionnaireCreationParam model) =>
            new()
            {
                TeacherEmail = model.TeacherEmail,
                SubjectName = model.SubjectName,
                StudentSetIds = [.. model.StudentSetIds]
            };
        public static Questionnaire ToModel(this QuestionnaireDto dto) =>
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
        public static QuestionnaireDto ToDto(this Questionnaire model) =>
            new()
            {
                SurveyId = model.SurveyId,

                TeacherEmail = model.TeacherEmail,
                StudentEmail = model.StudentEmail,
                SubjectName = model.SubjectName,
                QuestionnaireResults = model.QuestionnaireResults
                    .Select(q => q.ToDto())
                    .ToList() ?? new List<QuestionAnswerDto>()
            };
        public static QuestionAnswerDto ToDto(this QuestionAnswer model) =>
            new()
            {
                Answer = model.Answer

            };
        public static QuestionAnswer ToModel(this QuestionAnswerDto dto) =>
            new()
            {
                Answer = dto.Answer,
            };

        public static QuestionTemplate ToModel(this QuestionTemplateDto dto) =>
            new()
            {
                Question = dto.Question,
                Type = dto.Type,
                AnswerOptions = dto.AnswerOptions
            };
        public static QuestionTemplateDto ToDto(this QuestionTemplate model) =>
            new()
            {
                Question = model.Question,
                Type = model.Type,
                AnswerOptions = model.AnswerOptions
            };
        public static GetSurveyMetadataDto toDto(this SurveyMetadata model) =>
            new()
            {
                Id = model.Id,
                Title = model.Title,
                endDate = model.EndDate,
            };
    };

    }
