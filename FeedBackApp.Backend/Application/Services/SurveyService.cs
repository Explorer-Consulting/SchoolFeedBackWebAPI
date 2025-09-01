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

        public async Task<List<GetSurveyMetadataDTO>> GetAllSurveyMetadata()
        {
            var metadatas = await _questionnaireRepository.GetAllSurveyMetadata();
            var dtos = metadatas.Select(m => m.ToGetDto()).ToList();
            return dtos;
        }

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
