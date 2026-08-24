using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire.Post
{
    public class GenerateValidationTokenRequestDTO
    {
        [JsonProperty("studentEmail")]
        public string StudentEmail { get; set; } = string.Empty;
    }
}