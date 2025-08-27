
using Application.DTOs.Evaluation;
using FeedBackApp.Core.Model;

namespace Application.Extensions.EvaluationExtensions
{
    public static class EvaluationMappingExtension
    {
        public static Questionnaire ToModel(this UpdateQuestionnaireDTO dto) =>
            new()
            {
                QuestionnaireResults = dto.QuestionnaireResult
                    .Select(q => q.ToModel())
                    .ToList()
            };
        public static QuestionAnswer ToModel(this QuestionResultDTO dto) =>
            new()
            {
                Answer = dto.Answer,
                QuestionId = dto.QuestionId
            };
    }
}
