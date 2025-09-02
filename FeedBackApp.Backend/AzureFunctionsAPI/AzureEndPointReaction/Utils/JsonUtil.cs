using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace AzureFunctionsAPI.AzureEndPointReaction.Utils
{
    public class JsonUtil
    {
        public static async Task<T?> ReadFromJsonAsync<T>(HttpRequestData request)
        {
            using var reader = new StreamReader(request.Body);
            string body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
                return default;

            return JsonConvert.DeserializeObject<T>(body);
        }
    }
}
