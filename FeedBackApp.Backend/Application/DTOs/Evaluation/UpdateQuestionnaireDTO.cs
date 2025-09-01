using Newtonsoft.Json;

namespace Application.DTOs.Evaluation
{
    public class UpdateQuestionnaireDTO
    {
        [JsonProperty("questionnaireResult")]
        public List<QuestionResultDTO> QuestionnaireResult { get; set; } = new List<QuestionResultDTO>();

    }
}
