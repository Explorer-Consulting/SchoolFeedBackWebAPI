namespace Application.DTOs.Questionnaire.GetQuestionnaires
{
    public class QuestionnairesDTO
    {
        public string @Class { get; set; } = string.Empty;
        public List<SubjectDTO> Subjects { get; set; } = new List<SubjectDTO>();
    }
}
