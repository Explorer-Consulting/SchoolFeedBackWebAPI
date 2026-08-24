using Application.DTOs.Evaluation;
using Application.Extensions.EvaluationExtensions;
using Application.Services.Interfaces;
using Application.Validation.SubmitValidation;
using Application.Validation.UpdateValidation;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Repositories;
using FluentValidation;

namespace Application.Services
{
    /// <summary>
    /// Orchestrates questionnaire update and submission workflows, including validation,
    /// mapping from DTOs to domain entities, and repository persistence.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Responsibilities</b><br/>
    /// Retrieves existing questionnaire state, resolves question templates and survey deadlines,
    /// validates incoming DTOs using FluentValidation, maps DTOs to <see cref="Questionnaire"/>,
    /// and performs the appropriate repository action (update or submit).
    /// </para>
    /// <para>
    /// <b>Validation strategy</b><br/>
    /// Validation is performed against the active template set associated with the questionnaire's survey.
    /// Update and submit flows share infrastructure but use distinct validators to enforce their specific rules.
    /// </para>
    /// <para>
    /// <b>Deadline enforcement</b><br/>
    /// The service disallows operations after the survey end date (UTC), returning an error response.
    /// </para>
    /// </remarks>
    public class EvaluationService : IEvaluationService
    {
        private readonly IEvaluationRepository _repository;

        /// <summary>
        /// Creates a new <see cref="EvaluationService"/> with the required repository dependency.
        /// </summary>
        /// <param name="repository">Repository for questionnaire persistence and metadata access.</param>
        public EvaluationService(IEvaluationRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Applies non-terminal changes to an existing questionnaire.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method:
        /// </para>
        /// <list type="number">
        ///   <item><description>Loads the existing questionnaire by <paramref name="id"/>.</description></item>
        ///   <item><description>Resolves the associated question templates and survey end date.</description></item>
        ///   <item><description>Validates <paramref name="dto"/> using <see cref="UpdateQuestionnaireValidator"/>.</description></item>
        ///   <item><description>Maps the DTO to a domain <see cref="Questionnaire"/> and persists via <see cref="IEvaluationRepository.UpdateOrSubmitQuestionnaire(Questionnaire, Questionnaire)"/>.</description></item>
        ///   <item><description>Returns a typed response conveying success or validation/domain errors.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="id">Questionnaire identifier.</param>
        /// <param name="dto">Update payload for in-progress questionnaires.</param>
        /// <returns>Outcome including success flag and message.</returns>
        public async Task<UpdateResponseDTO> UpdateQuestionnaire(string id, UpdateQuestionnaireDTO dto)
        {
            return await HandleQuestionnaireAsync(
                id,
                dto,
                templates => new UpdateQuestionnaireValidator(templates),
                (newQ, oldQ) => _repository.UpdateOrSubmitQuestionnaire(newQ, oldQ),
                (success, qid, errors) => success
                    ? new UpdateResponseDTO(true, $"Questionnaire {qid} was updated successfully.")
                    : new UpdateResponseDTO(false, errors ?? $"Update questionnaire {qid} failed"),
                d => d.ToModel()
            );
        }

        /// <summary>
        /// Finalizes a questionnaire submission and marks it as completed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method:
        /// </para>
        /// <list type="number">
        ///   <item><description>Loads the existing questionnaire by <paramref name="id"/>.</description></item>
        ///   <item><description>Resolves templates and ensures the survey is still open.</description></item>
        ///   <item><description>Validates <paramref name="dto"/> using <see cref="SubmitQuestionnaireValidator"/>.</description></item>
        ///   <item><description>Maps to domain model, sets the existing questionnaire's <c>Status</c> to <c>true</c></item>
        ///   <item><description>Returns a typed response describing the result.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="id">Questionnaire identifier.</param>
        /// <param name="dto">Submission payload for completed questionnaires.</param>
        /// <returns>Outcome including success flag and message.</returns>
        public async Task<SubmitResponseDTO> SubmitQuestionnaire(string id, SubmitQuestionnaireDTO dto)
        {
            return await HandleQuestionnaireAsync(
                id,
                dto,
                templates => new SubmitQuestionnaireValidator(templates),
                async (newQ, oldQ) =>
                {
                    oldQ.Status = true;
                    return await _repository.UpdateOrSubmitQuestionnaire(newQ, oldQ);
                },
                (success, qid, errors) => success
                    ? new SubmitResponseDTO(true, $"Questionnaire {qid} was submitted successfully.")
                    : new SubmitResponseDTO(false, errors ?? $"Submit questionnaire {qid} failed"),
                d => d.ToModel()
            );
        }

        /// <summary>
        /// Shared orchestration routine for update/submit flows: loads state, validates DTO, maps to domain, and persists.
        /// </summary>
        /// <typeparam name="TDto">The DTO type carrying questionnaire data.</typeparam>
        /// <typeparam name="TResponse">The response type returned to the caller.</typeparam>
        /// <param name="id">Questionnaire identifier.</param>
        /// <param name="dto">Input DTO to validate and map.</param>
        /// <param name="validatorFactory">Factory that produces a FluentValidation validator for <typeparamref name="TDto"/> given the active templates.</param>
        /// <param name="repoActionAsync">Repository action that persists the mapped questionnaire against the existing one.</param>
        /// <param name="responseProvider">Factory to create a typed response from success flag, questionnaire id, and optional error.</param>
        /// <param name="mapToModel">Mapping function from <typeparamref name="TDto"/> to <see cref="Questionnaire"/>.</param>
        /// <returns>A typed response produced by <paramref name="responseProvider"/>.</returns>
        /// <remarks>
        /// <para>
        /// <b>Preconditions</b><br/>
        /// Requires an existing questionnaire referenced by <paramref name="id"/> and a valid template set
        /// associated with its survey. Fails if the survey end date is in the past.
        /// </para>
        /// <para>
        /// <b>Validation</b><br/>
        /// Uses a validator constructed from the resolved templates; returns a failed response if validation errors are present.
        /// </para>
        /// </remarks>
        private async Task<TResponse> HandleQuestionnaireAsync<TDto, TResponse>(
            string id,
            TDto dto,
            Func<IList<QuestionTemplate>, IValidator<TDto>> validatorFactory,
            Func<Questionnaire, Questionnaire, Task<bool>> repoActionAsync,
            Func<bool, string, string?, TResponse> responseProvider,
            Func<TDto, Questionnaire> mapToModel
        )
        where TDto : class
        {
            var oldQuestionnaire = await _repository.GetQuestionnaireByIdAsync(id);
            if (oldQuestionnaire == null)
                return responseProvider(false, id, $"Questionnaire {id} not found.");

            var questionTemplate = await _repository.GetQuestionTemplateBySurveyIdAsync(oldQuestionnaire.SurveyId);
            if (questionTemplate == null)
                return responseProvider(false, id, $"QuestionnaireTemplates {id} not found.");

            var endDate = await _repository.GetEndDateBySurveyId(oldQuestionnaire.SurveyId);
            if (endDate < DateTime.UtcNow)
                return responseProvider(false, id, endDate.ToString());

            var validator = validatorFactory(questionTemplate.QuestionTemplates);
            var validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return responseProvider(false, id, $"Validation failed: {errors}");
            }

            var model = mapToModel(dto);
            bool success = await repoActionAsync(model, oldQuestionnaire);
            return responseProvider(success, id, null);
        }
    }
}
