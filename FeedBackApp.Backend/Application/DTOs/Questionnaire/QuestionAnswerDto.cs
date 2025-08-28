using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire
{
    public class QuestionAnswerDTO
    {
        [JsonProperty("answer")]
        public string Answer { get; set; } = string.Empty;
    }
}