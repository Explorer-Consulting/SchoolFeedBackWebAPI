using Application.DTOs.Questionnaire.Post;
using FeedBackApp.Core.Model.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Application.DTOs.Questionnaire
{
    public class QuestionTemplateDTO
    {
        [JsonProperty("question")]
        public string Question { get; set; } = string.Empty;

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public QuestionType Type { get; set; }

        [JsonProperty("answerOptions")]
        public List<string> AnswerOptions { get; set; } = new();

        [JsonProperty("dependency")]
        public DependencyDTO? Dependency { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("requiredValidation")]
        public bool RequiredValidation { get; set; } = false;

    }
}
