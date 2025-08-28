
using Newtonsoft.Json;

namespace Application.DTOs.Evaluation
{
    public class BaseEvaluateQuestionnaireDTO
    {
        [JsonProperty("questionnaireResult")]
        public List<QuestionResultDTO> QuestionnaireResult { get; set; } = new List<QuestionResultDTO>();

    }
}
