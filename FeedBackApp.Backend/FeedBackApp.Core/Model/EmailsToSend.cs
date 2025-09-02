
namespace FeedBackApp.Core.Model
{
    public class EmailsToSend
    {
        public string Id { get; set; } = string.Empty;

        public IList<string> Emails { get; set; } = new List<string>();
    }
}
