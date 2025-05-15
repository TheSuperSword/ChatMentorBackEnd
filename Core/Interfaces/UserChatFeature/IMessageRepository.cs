using ChatMentor.Backend.Model.UserChat_Models;

namespace ChatMentor.Backend.Core.Interfaces.UserChatFeature;

public interface IMessageRepository
{
    // Add New Message
    Task<Message> CreateMessageAsync(Message message);
    
    // Get Messages by Conversation ID
    Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId, Guid? cursorMessageId = null,
        int limit = 20);
    
    // Retreive Message by ID
    Task<Message?> GetMessageByGuidAsync(Guid messageId);
    
    Task<Message?> GetMessagesByIdAsync(int messageId);
    
    // Update Message (Update the content and update the IsEdited flag and EditedAt timestamp)
    Task<bool> UpdateMessageAsync(Message message);
    
    // Delete Message (Set IsDeleted flag to true)
    Task<bool> DeleteMessageAsync(Guid messageId);
}