using ChatMentor.Backend.Model.UserChat_Models;

namespace ChatMentor.Backend.Core.Repositories.UserChatFeature;

/// <summary>
/// Repository interface for managing message attachments
/// </summary>
public interface IMessageAttachmentRepository
{
    Task<MessageAttachment> CreateAttachmentAsync(MessageAttachment attachment);
    Task<List<MessageAttachment>> GetAttachmentsByMessageIdAsync(int messageId);
    Task<bool> DeleteAttachmentAsync(int attachmentId);
    Task<bool> DeleteAttachmentsByMessageIdAsync(int messageId);
}