using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire
{
    public class CreateSurveyMetadataDTO
    {
        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("studentSets")]
        public List<StudentSetDTO> StudentSets { get; set; } = new();

        [JsonProperty("questionnaireTemplate")]
        public List<QuestionTemplateDTO> QuestionTemplates { get; set; } = new();

        [JsonProperty("teachers")]
        public List<MetaTeacherDTO> Teachers { get; set; } = new();
        
        [JsonProperty("questionnaireCreationParams")]
        public List<QuestionnaireCreationParamDTO> CreationParams { get; set; } = new();
    }
}
