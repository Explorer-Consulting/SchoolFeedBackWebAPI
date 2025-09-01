using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire.Post
{
    public class PostAnswerDto
    {
        [JsonProperty("answer")]
        public string Answer { get; set; } = string.Empty;
    }
}