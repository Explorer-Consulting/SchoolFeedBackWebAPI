using Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ModerationService : IModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ModerationService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["CohereApiKey"]!;
        }

        public async Task<bool> IsOffensiveAsync(string text)
        {
            var prompt = $"""
            Analyze the following student feedback.

            Return ONLY:
            Yes
            or
            No

            Return "Yes" ONLY if the text contains:
            - profanity
            - insults
            - harassment
            - hate speech
            - threats
            - abusive personal attacks

            Constructive criticism, disagreement, or negative feedback alone should NOT be considered offensive.

            Text:
            "{text}"
            """;

            var body = new
            {
                model = "command-nightly",
                message = prompt
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.cohere.com/v1/chat");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            request.Headers.Add("accept", "application/json");

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var result = doc.RootElement
                .GetProperty("text")
                .GetString();

            return result?
                .ToLower()
                .Contains("yes") == true;
        }
    }
}
