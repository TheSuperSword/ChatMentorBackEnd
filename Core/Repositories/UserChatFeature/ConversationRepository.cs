using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model.UserChat_Models;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories.UserChatFeature;

public class ConversationRepository : IConversationRepository
{
    private readonly ChatMentorDbContext _dbContext;

    public ConversationRepository(ChatMentorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Conversation> CreateConversationAsync(Conversation conversation)
    {
        await _dbContext.TblConversations.AddAsync(conversation);
        await _dbContext.SaveChangesAsync();
        return conversation;
    }

    public async Task<List<Conversation>> GetUserConversationAsync(int userId)
    {
        return await _dbContext.TblConversations
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .Include(c => c.Members)
            .ThenInclude(m => m.User)
            .OrderByDescending(c => c.LastMessageAt) // 🧠 Sort by most recent
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .ToListAsync();
    }

    public async Task<Conversation> GetConversationByIdAsync(Guid conversationId)
    {
        return await _dbContext.TblConversations
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId)
            ?? throw new KeyNotFoundException("Conversation not found.");
    }

    public async Task<List<ConversationMember>> GetConversationMembersAsync(Guid conversationId)
    {
        var conversation = await _dbContext.TblConversations
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        if (conversation == null) return new List<ConversationMember>();
        return conversation.Members;
    }

    public async Task<bool> UpdateConversationTitleAsync(Guid conversationId, string newTitle)
    {
        var conversation = await _dbContext.TblConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

        if (conversation == null) return false;

        conversation.Name = newTitle;
        _dbContext.TblConversations.Update(conversation);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLastUpdatedAtAsync(Guid conversationId)
    {
        var conversation = await _dbContext.TblConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

        if (conversation == null) return false;

        conversation.LastMessageAt = DateTime.UtcNow;
        _dbContext.TblConversations.Update(conversation);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteConversationAsync(Guid conversationId)
    {
        var conversation = await _dbContext.TblConversations
            .Include(c => c.Messages)
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

        if (conversation == null) return false;

        _dbContext.TblMessages.RemoveRange(conversation.Messages);
        _dbContext.TblConversationMembers.RemoveRange(conversation.Members);
        _dbContext.TblConversations.Remove(conversation);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Conversation?> GetOneToOneConversationAsync(int userAId, int userBId)
    {
        return await _dbContext.TblConversations
            .Where(c => !c.IsGroup && 
                        c.Members.Count == 2 && 
                        c.Members.Any(m => m.UserId == userAId) && 
                        c.Members.Any(m => m.UserId == userBId))
            .Include(c => c.Members)
            .FirstOrDefaultAsync();
    }
    
    public async Task<bool> AddUserToConversationAsync(Guid conversationId, int userId)
    {
        var conversation = await _dbContext.TblConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

        if (conversation == null) return false;

        var exists = await _dbContext.TblConversationMembers
            .AnyAsync(m => m.ConversationId == conversation.Id && m.UserId == userId);

        if (exists) return false;

        var member = new ConversationMember
        {
            ConversationId = conversation.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            Role = MemberRole.Regular
        };

        await _dbContext.TblConversationMembers.AddAsync(member);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveUserFromConversationAsync(Guid conversationId, int userId)
    {
        var conversation = await _dbContext.TblConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

        if (conversation == null) return false;

        var member = await _dbContext.TblConversationMembers
            .FirstOrDefaultAsync(m => m.ConversationId == conversation.Id && m.UserId == userId);

        if (member == null) return false;

        _dbContext.TblConversationMembers.Remove(member);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsUserInConversationAsync(Guid conversationId, int userId)
    {
        var conversation = await _dbContext.TblConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

        if (conversation == null) return false;

        return await _dbContext.TblConversationMembers
            .AnyAsync(m => m.ConversationId == conversation.Id && m.UserId == userId);
    }
    
    
}
