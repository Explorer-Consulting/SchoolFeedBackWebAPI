using Application.DTOs.Survey;
using Application.Extensions.QuestionnaireExtensions;
using Application.Services.Interfaces;
using FeedBackApp.Core.Repositories;

namespace Application.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly IQuestionnaireRepository _questionnaireRepository;

        public SurveyService(IQuestionnaireRepository questionnaireRepository)
        {
            _questionnaireRepository = questionnaireRepository;
        }
        public async Task<List<GetSurveyMetadataDto>> GetSurveyMetadataForStudent(string studentEmail)
        {
            var metadatas = await _questionnaireRepository.GetSurveyMetadataForStudentAsync(studentEmail);
            List<GetSurveyMetadataDto> dto = metadatas.Select(x => x.toDto()).ToList();
            return dto;
        }
    }
}
