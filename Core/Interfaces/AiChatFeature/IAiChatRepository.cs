using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Core.Interfaces;

public interface IAiChatRepository
{
    Task<AIChat> CreateChatAsync(AIChat chat);
    Task<List<AIChat>> GetChatsBySessionIdAsync(Guid sessionId, int pageNumber, int pageSize);    
    Task<AIChat?> GetLastChatInSessionAsync(Guid sessionId);
}