using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories;

public class AiChatRepository : IAiChatRepository
{
    private readonly ChatMentorDbContext _dbContext;
    
    public AiChatRepository(ChatMentorDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<AIChat> CreateChatAsync(AIChat chat)
    {
        _dbContext.TblAiChats.Add(chat);
        await _dbContext.SaveChangesAsync();
        return chat;
    }

    public async Task<List<AIChat>> GetChatsBySessionIdAsync(Guid sessionId, int pageNumber, int pageSize)
    {
        return await _dbContext.TblAiChats
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<AIChat?> GetLastChatInSessionAsync(Guid sessionId)
    {
        return await _dbContext.TblAiChats
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.Timestamp)
            .FirstOrDefaultAsync();
    }
}