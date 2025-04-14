using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Repositories.Interfaces;

public interface IUserTagRepository
{
    Task<IEnumerable<UserTag>> GetAllUserTagsAsync();
    Task<UserTag?> GetUserTagByIdAsync(int id);
    Task<IEnumerable<UserTag>> GetUserTagsByUserIdAsync(int userId);
    Task<IEnumerable<UserTag>> GetUserTagsByTagIdAsync(int tagId);
    Task<UserTag?> GetUserTagByUserIdAndTagIdAsync(int userId, int tagId);
    Task<UserTag> CreateUserTagAsync(UserTag userTag);
    Task<bool> DeleteUserTagAsync(int id);
    Task<bool> DeleteUserTagAsync(int userId, int tagId);
    Task<bool> UserTagExistsAsync(int id);
    Task<bool> UserTagExistsAsync(int userId, int tagId);
}