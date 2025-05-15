using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories;

public class AiChatSessionRepository : IAiChatSessionRepository
{
    private readonly ChatMentorDbContext _dbContext;
    private IAiChatSessionRepository _aiChatSessionRepositoryImplementation;

    public AiChatSessionRepository(ChatMentorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AiChatSession> CreateSessionAsync(AiChatSession session)
    {
        _dbContext.TblAiChatSessions.Add(session);
        await _dbContext.SaveChangesAsync();
        return session;
    }
    
    public async Task<List<AiChatSession>> GetSessionsByUserIdAsync(Guid userId, int pageNumber, int pageSize)
    {
        return await _dbContext.TblAiChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastUpdatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<AiChatSession?> GetSessionByIdAsync(Guid sessionId)
    {
        return await _dbContext.TblAiChatSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<bool> UpdateSessionTitleAsync(Guid sessionId, string newTitle)
    {
        var session = await _dbContext.TblAiChatSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Title = newTitle;
            session.LastUpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> UpdateLastUpdatedAtAsync(Guid sessionId)
    {
        var session = await _dbContext.TblAiChatSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.LastUpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        var session = await _dbContext.TblAiChatSessions.FindAsync(sessionId);
        if (session == null)
        {
            return false;
        }
    
        _dbContext.TblAiChatSessions.Remove(session);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
}