using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model.UserChat_Models;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories.UserChatFeature;

public class MessageAttachmentRepository : IMessageAttachmentRepository
{
    private readonly ChatMentorDbContext _dbContext;

    public MessageAttachmentRepository(ChatMentorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MessageAttachment> CreateAttachmentAsync(MessageAttachment attachment)
    {
        await _dbContext.TblMessageAttachments.AddAsync(attachment);
        await _dbContext.SaveChangesAsync();
        return attachment;
    }

    public async Task<List<MessageAttachment>> GetAttachmentsByMessageIdAsync(int messageId)
    {
        return await _dbContext.TblMessageAttachments
            .Where(a => a.MessageId == messageId)
            .Include(a => a.Document)
            .ToListAsync();
    }

    public async Task<bool> DeleteAttachmentAsync(int attachmentId)
    {
        var attachment = await _dbContext.TblMessageAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
            return false;

        _dbContext.TblMessageAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAttachmentsByMessageIdAsync(int messageId)
    {
        var attachments = await _dbContext.TblMessageAttachments
            .Where(a => a.MessageId == messageId)
            .ToListAsync();

        if (!attachments.Any())
            return false;

        _dbContext.TblMessageAttachments.RemoveRange(attachments);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}