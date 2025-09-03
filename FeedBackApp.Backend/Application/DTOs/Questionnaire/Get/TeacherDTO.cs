namespace Application.DTOs.Questionnaire
{
    public class TeacherDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public List<GetAnswerDTO> Answers { get; set; } = new List<GetAnswerDTO>();
    }
}
