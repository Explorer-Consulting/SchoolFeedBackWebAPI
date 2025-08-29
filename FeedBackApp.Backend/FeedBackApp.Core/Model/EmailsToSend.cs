
namespace FeedBackApp.Core.Model
{
    public class EmailsToSend
    {
        public string Id { get; set; } = string.Empty;

        public IList<string> EmailToSend { get; set; } = new List<string>();
    }
}
