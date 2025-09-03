using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire.Post
{
    public class DependencyDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("answerConditions")]
        public List<int> AnswerConditions { get; set; } = new List<int>();
    }
}
