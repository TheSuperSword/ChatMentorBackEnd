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

    // Creators
    Task<User> CreateUserAsync(User user);

    // Validators
    Task<bool> IsEmailInUseAsync(string email);
    Task<bool> IsGuidValidAsync(string guid);
}