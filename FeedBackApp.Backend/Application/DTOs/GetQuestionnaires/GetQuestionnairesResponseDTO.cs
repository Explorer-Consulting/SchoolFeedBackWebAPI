namespace Application.DTOs.GetQuestionnaires
{
    public class GetQuestionnairesResponseDTO
    {
        public string @Class { get; set; } = string.Empty;
        public List<SubjectDTO> Subjects { get; set; } = new List<SubjectDTO>();
    }
}
