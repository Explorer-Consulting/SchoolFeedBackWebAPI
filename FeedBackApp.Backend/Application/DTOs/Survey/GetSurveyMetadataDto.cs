
using Newtonsoft.Json;

namespace Application.DTOs.Survey
{
    public class GetSurveyMetadataDTO
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;
        [JsonProperty("endDate")]
        public DateTime endDate { get; set; }
    }
}
