using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Repositories;

public class UserTagRepository : IUserTagRepository
{
    private readonly ChatMentorDbContext _context;

    public UserTagRepository(ChatMentorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserTag>> GetAllUserTagsAsync()
    {
        return await _context.TblUserTag
            .Include(ut => ut.Tag)
            .ToListAsync();
    }

    public async Task<UserTag?> GetUserTagByIdAsync(int id)
    {
        return await _context.TblUserTag
            .Include(ut => ut.Tag)
            .FirstOrDefaultAsync(ut => ut.Id == id);
    }

    public async Task<IEnumerable<UserTag>> GetUserTagsByUserIdAsync(int userId)
    {
        return await _context.TblUserTag
            .Include(ut => ut.Tag)
            .Where(ut => ut.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserTag>> GetUserTagsByTagIdAsync(int tagId)
    {
        return await _context.TblUserTag
            .Include(ut => ut.User)
            .Where(ut => ut.TagId == tagId)
            .ToListAsync();
    }

    public async Task<UserTag?> GetUserTagByUserIdAndTagIdAsync(int userId, int tagId)
    {
        return await _context.TblUserTag
            .Include(ut => ut.Tag)
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TagId == tagId);
    }

    public async Task<UserTag> CreateUserTagAsync(UserTag userTag)
    {
        _context.TblUserTag.Add(userTag);
        await _context.SaveChangesAsync();
        return userTag;
    }

    public async Task<bool> DeleteUserTagAsync(int id)
    {
        var userTag = await _context.TblUserTag.FindAsync(id);
        if (userTag == null)
        {
            return false;
        }

        _context.TblUserTag.Remove(userTag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserTagAsync(int userId, int tagId)
    {
        var userTag = await _context.TblUserTag
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TagId == tagId);
            
        if (userTag == null)
        {
            return false;
        }

        _context.TblUserTag.Remove(userTag);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UserTagExistsAsync(int id)
    {
        return await _context.TblUserTag.AnyAsync(ut => ut.Id == id);
    }

    public async Task<bool> UserTagExistsAsync(int userId, int tagId)
    {
        return await _context.TblUserTag.AnyAsync(ut => ut.UserId == userId && ut.TagId == tagId);
    }
}