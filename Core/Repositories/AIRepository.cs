using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChatMentor.Backend.Core.Interfaces;

public class AIRepository : IAIRepository
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string? _apiUri;
    private readonly string? _model;
    
    
    public async Task<string> GenerateContentAsync(string prompt)
    {
        var request = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var jsonRequest = JsonSerializer.Serialize(request);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://ai-chengweelee00001740ai246813796837.services.ai.azure.com/models/chat/completions?api-version=2024-05-01-preview"),
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", _apiKey)
            },
            Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);
        var content = jsonDoc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? "No response.";
    }

    public async Task<string> GenerateCourseOutlineAsync(string courseTitle)
    {
        var prompt = $"Create a detailed course outline for: {courseTitle}";
        return await GenerateContentAsync(prompt);
    }

    public async Task<string> GenerateQuizAsync(string topic)
    {
        var prompt = $"Create a 5-question quiz for the topic: {topic}. Each question should have 4 options and highlight the correct one.";
        return await GenerateContentAsync(prompt);
    }
}
