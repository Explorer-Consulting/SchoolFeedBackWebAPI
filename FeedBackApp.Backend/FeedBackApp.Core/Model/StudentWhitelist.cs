
namespace FeedBackApp.Core.Model
{
    public class StudentWhitelist
    {
        public string Id { get; set; } = "StudentWhitelist";
        public List<string> StudentEmails { get; set; } = new List<string>();
    }
}
