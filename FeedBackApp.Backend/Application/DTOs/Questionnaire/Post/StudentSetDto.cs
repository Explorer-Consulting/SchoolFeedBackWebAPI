using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire
{
    public class StudentSetDTO
    {
        [JsonProperty("setId")]
        public string SetId { get; set; } = string.Empty;

        [JsonProperty("studentEmails")]
        public List<string> StudentEmails { get; set; } = new();
    }
}
