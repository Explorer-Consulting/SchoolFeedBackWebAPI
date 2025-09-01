using Application.DTOs.Survey;

namespace Application.Services.Interfaces
{
    public interface ISurveyService
    {
        public Task<List<GetSurveyMetadataDTO>> GetSurveyMetadataForStudent(string studentEmail);

        public Task<List<GetSurveyMetadataDTO>> GetAllSurveyMetadata();
    }
}
