using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Core.Interfaces;

public interface IAiChatSessionRepository
{
    Task<AiChatSession> CreateSessionAsync(AiChatSession session);
    Task<List<AiChatSession>> GetSessionsByUserIdAsync(Guid userId, int pageNumber, int pageSize);
    Task<AiChatSession?> GetSessionByIdAsync(Guid sessionId);
    Task<bool> UpdateSessionTitleAsync(Guid sessionId, string newTitle);
    Task<bool> UpdateLastUpdatedAtAsync(Guid sessionId);
    Task<bool> DeleteSessionAsync(Guid sessionId);
}