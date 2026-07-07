using System.Text;

namespace ValidatorMobileApp.Rest
{
    public class RestService
    {
        private static HttpClient _client = new HttpClient();

        public RestService()
        {
        }

        public static async Task<string> ValidateFromQRCodeAsync(string id)
        {
            var cucc = Environment.GetEnvironmentVariables();
            var domain = "https://studentfeedback-dev-api.azurewebsites.net";
#if DEBUG
            //domain = "http://192.168.0.171:7277";
#endif
            var url = $"{domain}/api/surveys";
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
