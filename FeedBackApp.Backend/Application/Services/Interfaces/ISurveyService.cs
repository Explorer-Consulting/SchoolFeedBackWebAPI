using Application.DTOs.Survey;

namespace Application.Services.Interfaces
{
    public interface ISurveyService
    {
        public List<GetSurveyMetadataDto> GetSurveyMetadataForStudent(string studentEmail);
    }
}
