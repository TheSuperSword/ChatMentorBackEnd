using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories;

public class UserRepository(ChatMentorDbContext context) : IUserRepository
{
    private IUserRepository _userRepositoryImplementation;

    // Retrievers
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await context.TblUser.FindAsync(id);
    }

    public async Task<User?> GetUserByGuidAsync(string guid)
    {
        if (!Guid.TryParse(guid, out var parsedGuid)) return null;
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

    public async Task<bool> UpdateLastLoginAsync(User user)
    {
        user.LastLogon = DateTime.UtcNow;
        context.TblUser.Update(user);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> IncrementPasswordTriesAsync(User user)
    {
        user.FailedLoginAttempts++;
        context.TblUser.Update(user);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ResetPasswordTriesAsync(User user)
    {
        user.FailedLoginAttempts = 0;
        context.TblUser.Update(user);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> LockUserAsync(User user)
    {
        user.Status = AccountStatus.Suspended;
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

    public async Task<bool> UserExistsAsyncByGuid(string guid)
    {
        if (!Guid.TryParse(guid, out var parsedGuid)) return false; // Handle invalid format early
        return await context.TblUser.AnyAsync(u => u.UserId == parsedGuid);
    }

    public async Task<bool> UserExistsAsyncById(int id)
    {
        return await context.TblUser.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> SetRefreshTokenAsync(User user, string? refreshToken, DateTime expiryTime)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiryTime;
        context.TblUser.Update(user);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await context.TblUser.AsNoTracking()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow);
    }
}