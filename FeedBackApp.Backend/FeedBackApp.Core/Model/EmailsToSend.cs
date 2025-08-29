
using Newtonsoft.Json;

namespace FeedBackApp.Core.Model
{
    public class EmailsToSend
    {
        public string Id { get; set; } = string.Empty;

        public IList<string> EmaailToSend { get; set; } = new List<string>();
    }
}
