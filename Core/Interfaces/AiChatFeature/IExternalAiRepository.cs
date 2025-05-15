using OpenAI.Chat;

namespace ChatMentor.Backend.Core.Interfaces;

public interface IExternalAiRepository
{
    Task<string> GetAiResponseAsync(string userMessage);
    Task<string> GetAiResponseWithChatHistoryAsync(List<ChatMessage> chatHistory);
}