using ChatMentor.Backend.Model.UserChat_Models;

namespace ChatMentor.Backend.Core.Interfaces.UserChatFeature;

public interface IUserConnectionsRepository
{
    // Get all connections 
    Task<List<UserConnection>> GetAllConnectionsAsync();
    // Add connection
    Task<UserConnection> AddConnectionAsync(UserConnection connection);
    // Remove connection
    Task<bool> RemoveConnectionAsync(string connectionId);
}