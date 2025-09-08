
namespace FeedBackApp.Core.Model
{
    public class StudentWhitelist
    {
        public string Id = "StudentWhitelist";
        public List<string> StudentEmails { get; set; } = new List<string>();
    }
}
