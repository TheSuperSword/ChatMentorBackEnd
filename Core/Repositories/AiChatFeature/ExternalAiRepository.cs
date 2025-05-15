using Azure;
using Azure.AI.OpenAI;
using ChatMentor.Backend.Core.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace ChatMentor.Backend.Core.Repositories;

public class ExternalAiRepository : IExternalAiRepository
{
    private readonly ChatClient _azureClient;
    private readonly OpenAiSettings _settings;

    public ExternalAiRepository(IOptions<OpenAiSettings> openAiSettings)
    {
        _settings = openAiSettings.Value;

        var endpoint = new Uri(_settings.ApiUri);
        var apiKey = _settings.ApiKey;
        var deploymentName = _settings.ModelName;

        AzureOpenAIClient azureClient = new(endpoint, new AzureKeyCredential(apiKey));
        _azureClient = azureClient.GetChatClient(deploymentName);
    }

    public async Task<string> GetAiResponseAsync(string userMessage)
    {
        var requestOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _settings.MaxTokens ?? 4096,
            Temperature = _settings.Temperature ?? 1.0f,
            TopP = _settings.TopP ?? 1.0f // Adjust if needed
        };
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage(userMessage)
        };
        var response = await _azureClient.CompleteChatAsync(messages, requestOptions);
        if (response.Value.Content[0] != null) return response.Value.Content[0].Text;
        throw new Exception("No response content received from AI service");
    }

    public async Task<string> GetAiResponseWithChatHistoryAsync(List<ChatMessage> messages)
    {
        var requestOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _settings.MaxTokens ?? 4096,
            Temperature = _settings.Temperature ?? 1.0f,
            TopP = _settings.TopP ?? 1.0f // Adjust if needed
        };
        var response = await _azureClient.CompleteChatAsync(messages, requestOptions);
        if (response.Value.Content[0] != null) return response.Value.Content[0].Text;
        throw new Exception("No response content received from AI service");
    }
}