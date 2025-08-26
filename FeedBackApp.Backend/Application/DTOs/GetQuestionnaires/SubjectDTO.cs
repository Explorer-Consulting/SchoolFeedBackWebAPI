
namespace Application.DTOs.GetQuestionnaires
{
    public class SubjectDTO
    {
        public string Name { get; set; } = string.Empty;
        public List<TeacherDTO> Teachers { get; set; } = new List<TeacherDTO>();
    }
}
