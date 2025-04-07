using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ChatMentorDbContext _context;

    public TagRepository(ChatMentorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _context.TblTag
            .Include(t => t.UserTags)
            .ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        return await _context.TblTag
            .Include(t => t.UserTags)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _context.TblTag
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task AddAsync(Tag tag)
    {
        await _context.TblTag.AddAsync(tag);
    }

    public async Task<bool> TagExistsAsync(string name)
    {
        return await _context.TblTag.AnyAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}