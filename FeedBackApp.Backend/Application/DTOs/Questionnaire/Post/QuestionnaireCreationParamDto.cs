using Newtonsoft.Json;

namespace Application.DTOs.Questionnaire
{
    public class QuestionnaireCreationParamDto
    {
        [JsonProperty("teacherEmail")]
        public string TeacherEmail { get; set; } = string.Empty;

        [JsonProperty("subjectName")]
        public string SubjectName { get; set; } = string.Empty;

        [JsonProperty("studentSetIds")]
        public List<string> StudentSetIds { get; set; } = new();
    }
}
