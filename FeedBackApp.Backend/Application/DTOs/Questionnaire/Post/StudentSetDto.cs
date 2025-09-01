using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire.Post
{
    public class StudentSetDto
    {
        [JsonProperty("setId")]
        public string SetId { get; set; } = string.Empty;

        [JsonProperty("studentEmails")]
        public List<string> StudentEmails { get; set; } = new();
    }
}
