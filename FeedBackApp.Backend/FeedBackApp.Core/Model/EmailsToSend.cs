
namespace FeedBackApp.Core.Model
{
    public class EmailsToSend
    {
        public string Id { get; set; } = string.Empty;

        public IList<Email> EmailsToSendList { get; set; } = new List<Email>();
    }
}
