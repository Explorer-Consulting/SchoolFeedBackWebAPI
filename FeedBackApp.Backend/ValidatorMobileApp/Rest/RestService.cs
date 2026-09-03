using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using ValidatorMobileApp.Config;

namespace ValidatorMobileApp.Rest
{
    public class RestService
    {
        private static readonly CookieContainer CookieContainer = new();

        private static readonly HttpClientHandler Handler = new()
        {
            CookieContainer = CookieContainer,
            UseCookies = true
        };

        private static readonly HttpClient _client = new(Handler);

        public static async Task<string> ValidateFromQRCodeAsync(string token)
        {
            var baseUrl = AppConfig.BaseUrl;

            CookieContainer.Add(
               new Uri(baseUrl),
               new Cookie("token", AppConfig.ApiKey)
            );

            var url = $"{baseUrl}/api/questionnaires/{token}/validate";
            Uri uri = new Uri(string.Format(url, string.Empty));
            try
            {
                StringContent content = new StringContent(token, Encoding.UTF8, "application/json");

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
