using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatMentor.Backend.Infrastructure.Repositories
{
    public class UserTagRepository : IUserTagRepository
    {
        private readonly ChatMentorDbContext _dbContext;

        public UserTagRepository(ChatMentorDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<IEnumerable<UserTag>> GetAllByUserIdAsync(int userId)
        {
            return await _dbContext.TblUserTag
                .Include(ut => ut.Tag)  // Include related Tag
                .Where(ut => ut.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserTag?> GetByUserIdAndTagIdAsync(int userId, int tagId)
        {
            return await _dbContext.TblUserTag
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TagId == tagId);
        }

        public async Task AddAsync(UserTag userTag)
        {
            await _dbContext.TblUserTag.AddAsync(userTag);
        }

        public Task Remove(UserTag userTag)
        {
            _dbContext.TblUserTag.Remove(userTag);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}