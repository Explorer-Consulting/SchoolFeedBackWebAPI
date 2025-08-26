using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire
{
    public class CreateSurveyMetadataDto
    {
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("studentSets")]
        public List<StudentSetDto> StudentSets { get; set; } = new();

        [JsonProperty("questionnaireTemplate")]
        public List<QuestionTemplateDto> QuestionTemplates { get; set; } = new();

        [JsonProperty("teachers")]
        public List<MetaTeacherDto> Teachers { get; set; } = new();
        
        [JsonProperty("questionnaireCreationParams")]
        public List<QuestionnaireCreationParamDto> CreationParams { get; set; } = new();
    }
}
