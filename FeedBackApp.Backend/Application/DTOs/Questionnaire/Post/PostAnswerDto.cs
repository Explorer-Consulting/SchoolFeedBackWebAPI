using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire
{
    public class PostAnswerDto
    {
        [JsonProperty("answer")]
        public string Answer { get; set; } = string.Empty;
    }
}