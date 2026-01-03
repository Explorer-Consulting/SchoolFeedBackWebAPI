using Application.DTOs.Evaluation;
using FeedBackApp.Core.Model;

namespace Application.Extensions.EvaluationExtensions
{
    /// <summary>
    /// Provides mapping extension methods for converting evaluation-related DTOs
    /// (<see cref="SubmitQuestionnaireDTO"/>, <see cref="UpdateQuestionnaireDTO"/>, <see cref="QuestionResultDTO"/>)
    /// into their corresponding domain model entities (<see cref="Questionnaire"/>, <see cref="QuestionAnswer"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Purpose</b><br/>
    /// Centralizes the logic for transforming data transfer objects used by API endpoints
    /// into domain entities that can be persisted or processed by the application core.
    /// These extensions ensure structural consistency and separation between transport- and domain-level representations.
    /// </para>
    ///
    /// <para>
    /// <b>Usage</b><br/>
    /// Typically invoked inside application services (e.g., <c>EvaluationService</c>) when a student submits or updates a questionnaire:
    /// <code>
    /// var model = submitDto.ToModel();
    /// await repository.SaveAsync(model);
    /// </code>
    /// </para>
    ///
    /// <para>
    /// <b>Behavior</b><br/>
    /// Both <see cref="SubmitQuestionnaireDTO"/> and <see cref="UpdateQuestionnaireDTO"/> share the same mapping logic:
    /// they convert their nested <c>QuestionnaireResult</c> collections into domain <see cref="QuestionAnswer"/> objects.
    /// The mapping is one-to-one and does not perform validation; semantic checks are handled elsewhere.
    /// </para>
    ///
    /// <para>
    /// <b>Notes</b>
    /// <list type="bullet">
    ///   <item><description>Each <see cref="QuestionResultDTO"/> is converted into a <see cref="QuestionAnswer"/> entity with corresponding <c>Answer</c> and <c>QuestionId</c> values.</description></item>
    ///   <item><description>The extension methods assume that the source DTO collections are non-null; callers should validate inputs before mapping.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static class EvaluationMappingExtension
    {
        /// <summary>
        /// Converts an <see cref="UpdateQuestionnaireDTO"/> into a domain <see cref="Questionnaire"/> entity.
        /// </summary>
        /// <param name="dto">DTO containing updated questionnaire data.</param>
        /// <returns>A mapped <see cref="Questionnaire"/> entity populated with converted <see cref="QuestionAnswer"/> results.</returns>
        public static Questionnaire ToModel(this UpdateQuestionnaireDTO dto) =>
            new()
            {
                QuestionnaireResults = dto.QuestionnaireResult
                    .Select(q => q.ToModel())
                    .ToList()
            };

        /// <summary>
        /// Converts a <see cref="SubmitQuestionnaireDTO"/> into a domain <see cref="Questionnaire"/> entity.
        /// </summary>
        /// <param name="dto">DTO containing submitted questionnaire data.</param>
        /// <returns>A mapped <see cref="Questionnaire"/> entity populated with converted <see cref="QuestionAnswer"/> results.</returns>
        public static Questionnaire ToModel(this SubmitQuestionnaireDTO dto) =>
            new()
            {
                QuestionnaireResults = dto.QuestionnaireResult
                    .Select(q => q.ToModel())
                    .ToList()
            };

        /// <summary>
        /// Converts a <see cref="QuestionResultDTO"/> into a domain <see cref="QuestionAnswer"/> entity.
        /// </summary>
        /// <param name="dto">DTO representing a student's answer to a specific question.</param>
        /// <returns>A <see cref="QuestionAnswer"/> instance containing the mapped <c>Answer</c> and <c>QuestionId</c>.</returns>
        public static QuestionAnswer ToModel(this QuestionResultDTO dto) =>
            new()
            {
                Answer = dto.Answer,
                QuestionId = dto.QuestionId
            };
    }
}
