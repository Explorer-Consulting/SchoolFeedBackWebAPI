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

        private static readonly string API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImR1bW15ZW1haWxAZW1haWwuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE4NzgxODI5OTcsImlzcyI6IlNjaG9vbEZlZWRiYWNrV2ViQVBJIiwiYXVkIjoiU2Nob29sRmVlZGJhY2tXZWJBUEkifQ.inYLkbfQknwlUExopNpqZBMiI_6Eib0Zm2tK7Khf2T0";

        public static async Task<string> ValidateFromQRCodeAsync(string id)
        {
            var baseUrl = AppConfig.BaseUrl;

            CookieContainer.Add(
               new Uri(baseUrl),
               new Cookie("token", API_KEY)
            );

            var url = $"{baseUrl}/api/surveys/{id}/validate";
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
