using System.Text;

namespace ValidatorMobileApp
{
    public class RestService
    {
        private static HttpClient _client = new HttpClient();

        public RestService()
        {
        }

        public static async Task<string> ValidateFromQRCodeAsync(string id)
        {
            var url = $"http://192.168.0.171:7277/api/surveys/{id}/validate";
            Uri uri = new Uri(string.Format(url, string.Empty));
            try
            {
                StringContent content = new StringContent(id, Encoding.UTF8, "application/json");

                var response = await _client.PatchAsync(uri, content);

                return response.IsSuccessStatusCode ? "success" : "fail";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "error";
            }
        }
    }
}
