using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Core.Interfaces;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(int id);
    Task<Tag?> GetByNameAsync(string name);
    Task AddAsync(Tag tag);
    Task<bool> TagExistsAsync(string name);
    Task SaveChangesAsync();
}
