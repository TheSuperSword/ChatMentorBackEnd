using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Repositories.Interfaces;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllTagsAsync();
    Task<Tag?> GetTagByIdAsync(int id);
    Task<Tag?> GetTagByNameAsync(string name);
    Task<Tag> CreateTagAsync(Tag tag);
    Task<Tag?> UpdateTagAsync(int id, Tag tag);
    Task<bool> DeleteTagAsync(int id);
    Task<bool> TagExistsAsync(string name);
    Task<bool> TagExistsAsync(int id);
}