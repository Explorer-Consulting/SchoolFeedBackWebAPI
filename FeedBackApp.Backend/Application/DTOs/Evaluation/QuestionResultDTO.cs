using Newtonsoft.Json;

namespace Application.DTOs.Evaluation
{
    public class QuestionResultDTO
    {
        [JsonProperty("questionId")]
        public string QuestionId { get; set; } = string.Empty;

        [JsonProperty("answer")]
        public string Answer { get; set; } = string.Empty;
    }
}
