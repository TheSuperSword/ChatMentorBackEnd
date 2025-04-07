using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Core.Interfaces;

public interface IUserTagRepository
{
    Task<IEnumerable<UserTag>> GetAllByUserIdAsync(int userId);  // Get all tags for a specific user
    Task<UserTag?> GetByUserIdAndTagIdAsync(int userId, int tagId);  // Get user-tag by user ID and tag ID
    Task AddAsync(UserTag userTag);  // Add a new user-tag relationship
    Task Remove(UserTag userTag);  // Remove a user-tag relationship
    Task SaveChangesAsync();  // Commit changes to the database
}