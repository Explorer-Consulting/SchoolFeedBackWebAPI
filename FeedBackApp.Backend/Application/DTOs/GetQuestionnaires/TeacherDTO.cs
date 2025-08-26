
namespace Application.DTOs.GetQuestionnaires
{
    public class TeacherDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public List<AnswerDTO> Answers { get; set; } = new List<AnswerDTO>();
    }
}
