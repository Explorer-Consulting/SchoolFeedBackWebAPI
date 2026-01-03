using Application.DTOs.Survey;
using Application.Extensions.QuestionnaireExtensions;
using Application.Services.Interfaces;
using FeedBackApp.Core.Repositories;

namespace Application.Services
{
    /// <summary>
    /// Read-only survey metadata query service.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides projection helpers for listing surveys either for administrators (all surveys)
    /// or for a specific student (time-windowed and audience-scoped). Domain entities are
    /// projected to lightweight transport models via mapping extensions.
    /// </para>
    /// <para>
    /// <b>Time filtering</b><br/>
    /// Student-scoped queries filter by the survey's <c>StartDate</c> (must be ≤ <see cref="DateTime.UtcNow"/>)
    /// and <c>EndDate</c> (must be ≥ <see cref="DateTime.UtcNow"/>), effectively returning only currently active items.
    /// </para>
    /// </remarks>
    public class SurveyService : ISurveyService
    {
        private readonly IQuestionnaireRepository _questionnaireRepository;

        /// <summary>
        /// Creates a new <see cref="SurveyService"/> backed by a questionnaire repository.
        /// </summary>
        /// <param name="questionnaireRepository">Repository providing access to survey metadata and audience scoping.</param>
        public SurveyService(IQuestionnaireRepository questionnaireRepository)
        {
            _questionnaireRepository = questionnaireRepository;
        }

        /// <summary>
        /// Returns all survey metadata records, projected to lightweight DTOs.
        /// </summary>
        /// <remarks>
        /// This is typically used by administrative views where no audience/time filtering is required.
        /// </remarks>
        /// <returns>List of <see cref="GetSurveyMetadataDTO"/> representing all surveys.</returns>
        public async Task<List<GetSurveyMetadataDTO>> GetAllSurveyMetadata()
        {
            var metadatas = await _questionnaireRepository.GetAllSurveyMetadata();
            var dtos = metadatas.Select(m => m.ToGetDto()).ToList();
            return dtos;
        }

        /// <summary>
        /// Returns the list of surveys visible to a given student that are currently active.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The repository scopes metadata by the student's email. The result is further filtered to
        /// surveys whose <c>StartDate</c> has passed and <c>EndDate</c> has not elapsed yet (UTC).
        /// </para>
        /// </remarks>
        /// <param name="studentEmail">Student email used for audience scoping.</param>
        /// <returns>Active <see cref="GetSurveyMetadataDTO"/> items for the student.</returns>
        public async Task<List<GetSurveyMetadataDTO>> GetSurveyMetadataForStudent(string studentEmail)
        {
            var metadatas = await _questionnaireRepository.GetSurveyMetadataForStudentAsync(studentEmail);
            metadatas = metadatas.Where(m => m.StartDate <= DateTime.UtcNow).ToList();
            metadatas = metadatas.Where(m => m.EndDate >= DateTime.UtcNow).ToList();
            List<GetSurveyMetadataDTO> dto = metadatas.Select(x => x.ToGetDto()).ToList();
            return dto;
        }
    }
}
