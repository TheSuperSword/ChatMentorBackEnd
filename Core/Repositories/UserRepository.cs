using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.DbContext;
using ChatMentor.Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories
{
    public class UserRepository(ChatMentorDbContext context) : IUserRepository
    {
        // Retrievers
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await context.TblUser.FindAsync(id);
        }

        public async Task<User?> GetUserByGuidAsync(string guid)
        {
            if (!Guid.TryParse(guid, out Guid parsedGuid)) return null;
            return await context.TblUser.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == parsedGuid);
        }
        
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await context.TblUser.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await context.TblUser.AsNoTracking().ToListAsync();
        }

        // Updaters
        public async Task<bool> UpdateUserAsync(User user)
        {
            context.TblUser.Update(user);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserPasswordAsync(User user, string hashedPassword)
        {
            user.PasswordHash = hashedPassword;
            context.TblUser.Update(user);
            return await context.SaveChangesAsync() > 0;
        }
        
        // Creators
        public async Task<User> CreateUserAsync(User user)
        {
            context.TblUser.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        // Validators
        public async Task<bool> IsEmailInUseAsync(string email)
        {
            return await context.TblUser.AnyAsync(u => u.Email == email);
        }
        
        public async Task<bool> IsGuidValidAsync(string guid)
        {
            if (!Guid.TryParse(guid, out Guid parsedGuid)) return false; // Handle invalid format early
            return await context.TblUser.AnyAsync(u => u.UserId == parsedGuid);
        }

        
    }
}
