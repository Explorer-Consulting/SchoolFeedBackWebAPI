using Application.DTOs.Evaluation;
using Application.DTOs.Questionnaire;
using FeedBackApp.Core.Model;

namespace Application.Extensions.QuestionnaireExtensions
{
    public static class SurveyMetadataMappingExtension
    {
        public static SurveyMetadata ToModel(this CreateSurveyMetadataDTO dto) =>
            new()
            {
                Id = Guid.NewGuid().ToString(),
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
                TeacherEmail = dto.TeacherEmail,
                StudentEmail = dto.StudentEmail,
                SubjectName = dto.SubjectName,
                Status = false,
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
                    .ToList() ?? new List<QuestionAnswerDTO>()
            };
        public static QuestionAnswerDTO ToDto(this QuestionAnswer model) =>
            new()
            {
                Answer = model.Answer

            };
        public static QuestionAnswer ToModel(this QuestionAnswerDTO dto) =>
            new()
            {
                Answer = dto.Answer,
            };

        public static QuestionTemplate ToModel(this QuestionTemplateDTO dto) =>
            new()
            {
                Question = dto.Question,
                Type = dto.Type,
                AnswerOptions = dto.AnswerOptions
            };
        public static QuestionTemplateDTO ToDto(this QuestionTemplate model) =>
            new()
            {
                Question = model.Question,
                Type = model.Type,
                AnswerOptions = [..model.AnswerOptions]
            }; 
    };
}
