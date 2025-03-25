using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.DbContext;
using ChatMentor.Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ChatMentorDbContext _context;

        public UserRepository(ChatMentorDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.TblUser.FindAsync(id);
        }

        public async Task<User?> GetUserByGuidAsync(string guid)
        {
            if (!Guid.TryParse(guid, out Guid parsedGuid))
                return null; // Return null or handle invalid GUID format

            return await _context.TblUser.FirstOrDefaultAsync(u => u.UserId == parsedGuid);
        }


        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.TblUser.ToListAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.TblUser.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.TblUser.FindAsync(id);
            if (user != null)
            {
                _context.TblUser.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}