using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Core.Interfaces;

public interface IUserRepository
{
    // Retrievers
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByGuidAsync(string guid);
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllUsersAsync();

    // Updaters  
    Task<bool> UpdateUserAsync(User user);
    Task<bool> UpdateUserPasswordAsync(User user, string newPassword);
    Task<bool> UpdateLastLoginAsync(User user);

    // Password Tries/ Lockout
    Task<bool> IncrementPasswordTriesAsync(User user);
    Task<bool> ResetPasswordTriesAsync(User user);
    Task<bool> LockUserAsync(User user);

    // Creators
    Task<User> CreateUserAsync(User user);

    // Validators
    Task<bool> IsEmailInUseAsync(string email);
    Task<bool> UserExistsAsyncByGuid(string guid);
    Task<bool> UserExistsAsyncById(int id);

    // Token Management
    Task<bool> SetRefreshTokenAsync(User user, string? refreshToken, DateTime expiryTime);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
}