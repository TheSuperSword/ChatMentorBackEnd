using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ChatMentorDbContext _context;

    public TagRepository(ChatMentorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        return await _context.TblTag.ToListAsync();
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        return await _context.TblTag.FindAsync(id);
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        return await _context.TblTag
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        _context.TblTag.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag?> UpdateTagAsync(int id, Tag tag)
    {
        var existingTag = await _context.TblTag.FindAsync(id);

        if (existingTag == null) return null;

        existingTag.Name = tag.Name;

        // No need to update CreatedAt and CreatedBy
        // UpdatedAt and UpdatedBy will be handled by SaveChangesAsync in DbContext

        await _context.SaveChangesAsync();
        return existingTag;
    }

    public async Task<bool> DeleteTagAsync(int id)
    {
        var tag = await _context.TblTag.FindAsync(id);
        if (tag == null) return false;

        _context.TblTag.Remove(tag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TagExistsAsync(string name)
    {
        return await _context.TblTag.AnyAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> TagExistsAsync(int id)
    {
        return await _context.TblTag.AnyAsync(t => t.Id == id);
    }
}