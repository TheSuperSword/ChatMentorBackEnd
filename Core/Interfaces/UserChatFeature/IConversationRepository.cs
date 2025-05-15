using ChatMentor.Backend.Model.UserChat_Models;

namespace ChatMentor.Backend.Core.Interfaces.UserChatFeature;

public interface IConversationRepository
{
    // CRUD operations for Conversation
    // Create a new conversation
    Task<Conversation> CreateConversationAsync(Conversation conversation);
    // Get all conversations for a user
    Task<List<Conversation>> GetUserConversationAsync(int userId);
    // Get a specific conversation by ID
    Task<Conversation> GetConversationByIdAsync(Guid conversationId);
    // Get all members of a conversation
    Task<List<ConversationMember>> GetConversationMembersAsync(Guid conversationId);
    // Get all conversations for a user with pagination
    Task<bool> UpdateConversationTitleAsync(Guid conversationId, string newTitle);
    // Update the title of a conversation
    Task<bool> UpdateLastUpdatedAtAsync(Guid conversationId);
    // Update the last updated timestamp of a conversation
    Task<bool> DeleteConversationAsync(Guid conversationId);
    Task<bool> IsUserInConversationAsync(Guid conversationId, int userId);
    
    // Check 1:1 conversation exists
    Task<Conversation?> GetOneToOneConversationAsync(int userAId, int userBId);
    // Delete a conversation
    Task<bool> AddUserToConversationAsync(Guid conversationId, int userId);
    // Add a user to a conversation
    Task<bool> RemoveUserFromConversationAsync(Guid conversationId, int userId);
    
}