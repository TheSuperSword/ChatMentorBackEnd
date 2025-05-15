using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model.UserChat_Models;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories.UserChatFeature;

public class MessageRepository : IMessageRepository
{
    private readonly ChatMentorDbContext _dbContext;

    public MessageRepository(ChatMentorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Message> CreateMessageAsync(Message message)
    {
        await _dbContext.TblMessages.AddAsync(message);
        await _dbContext.SaveChangesAsync();
        return message;
    }

    public async Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId, Guid? cursorMessageId = null, int limit = 20)
    {
        // Ensure ConversationId is resolved to Id first
        var conversation = await _dbContext.TblConversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

        if (conversation == null)
            return [];

        var query = _dbContext.TblMessages
            .Where(m => m.ConversationId == conversation.Id && !m.IsDeleted);
    
        // Apply cursor if provided
        if (cursorMessageId.HasValue)
        {
            // First get the SentAt time of the cursor message
            var cursorMessage = await _dbContext.TblMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MessageId == cursorMessageId.Value);

            if (cursorMessage != null)
            {
                // Find messages older than the cursor message or same time but with lower MessageId
                query = query.Where(m => 
                    m.SentAt < cursorMessage.SentAt || 
                    (m.SentAt == cursorMessage.SentAt && m.MessageId.CompareTo(cursorMessageId.Value) < 0));
            }
        }

        return await query
            .OrderByDescending(m => m.SentAt)
            .ThenByDescending(m => m.MessageId) // Secondary sort by MessageId ensures consistent ordering
            .Take(limit)
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
            .ToListAsync();
    }

    public async Task<Message?> GetMessageByGuidAsync(Guid messageId)
    {
        return await _dbContext.TblMessages
            .Include(m => m.Conversation)
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
            .FirstOrDefaultAsync(m => m.MessageId == messageId && !m.IsDeleted);
    }
    
    public async Task<Message?> GetMessagesByIdAsync(int messageId)
    {
        return await _dbContext.TblMessages
            .Include(m => m.Sender)
            .Include(m => m.ReplyToMessage)
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);
    }

    public async Task<bool> UpdateMessageAsync(Message updatedMessage)
    {
        var existingMessage = await _dbContext.TblMessages
            .FirstOrDefaultAsync(m => m.Id == updatedMessage.Id);

        if (existingMessage == null || existingMessage.IsDeleted)
            return false;

        existingMessage.Content = updatedMessage.Content;
        existingMessage.IsEdited = true;
        existingMessage.EditedAt = DateTime.UtcNow;

        _dbContext.TblMessages.Update(existingMessage);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId)
    {
        var message = await _dbContext.TblMessages
            .FirstOrDefaultAsync(m => m.MessageId == messageId);

        if (message == null || message.IsDeleted)
            return false;

        message.IsDeleted = true;
        _dbContext.TblMessages.Update(message);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
