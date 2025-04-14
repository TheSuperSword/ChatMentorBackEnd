using System.ComponentModel.DataAnnotations;
using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Responses;
using ChatMentor.Backend.Services;

namespace ChatMentor.Backend.Core.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserTagService _userTagService; // Add UserTagService

        public UserService(IUserRepository userRepository, UserTagService userTagService)
        {
            _userRepository = userRepository;
            _userTagService = userTagService; // Inject UserTagService
        }

        // Helper method for mapping User to UserDto
        private async Task<UserDto> MapToDtoAsync(User user)
        {
            // Get tags for this user
            var userTagsInfo = await _userTagService.GetTagsForUserAsync(user.Id);
            
            return new UserDto
            {
                Id = user.Id,
                UserGuid = user.UserId.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Headline = user.Headline,
                Bio = user.Bio,
                Role = user.Role,
                Tags = userTagsInfo.Tags // Add the tags list to the DTO
            };
        }

        // Retrievers - Updated to use async version of MapToDto
        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            return user is not null ? await MapToDtoAsync(user) : null;
        }

        public async Task<UserDto?> GetUserByGuidAsync(string guid)
        {
            var user = await _userRepository.GetUserByGuidAsync(guid);
            return user is not null ? await MapToDtoAsync(user) : null;
        }

        public async Task<(IEnumerable<UserDto> Users, PaginationMeta Meta)> GetPaginatedUsersAsync(int page, int pageSize)
        {
            if (page <= 0 || pageSize <= 0)
            {
                throw new ArgumentException("Page and pageSize must be greater than 0.");
            }

            var users = await _userRepository.GetAllUsersAsync(); // Fetch all users 

            var totalRecords = users.Count(); // Get total record count
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Adjust out-of-range page number
            if (page > totalPages && totalRecords > 0)
            { 
                throw new ArgumentException("Page number is out of range.");
            }

            var paginatedUsers = users.Skip((page - 1) * pageSize).Take(pageSize);

            // Generate pagination metadata
            var meta = new PaginationMeta(page, pageSize, totalRecords);

            // Map users to DTOs including their tags
            var userDtos = new List<UserDto>();
            foreach (var user in paginatedUsers)
            {
                userDtos.Add(await MapToDtoAsync(user));
            }

            return (userDtos, meta);
        } 
        
        // Updaters
        public async Task<bool> UpdateUserProfileAsync(string userGuid, UpdateUserDto dto)
        {
            var user = await _userRepository.GetUserByGuidAsync(userGuid);
            if (user is null) return false; // User not found

            // Check if email is being updated
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                // Validate email format
                if (!new EmailAddressAttribute().IsValid(dto.Email))
                    throw new ArgumentException("Invalid email format.");

                // Ensure email is not already in use
                var isEmailTaken = await _userRepository.IsEmailInUseAsync(dto.Email);
                if (isEmailTaken) throw new InvalidOperationException("Email is already in use.");
        
                user.Email = dto.Email; // Set new email
            }

            // Update other fields only if provided
            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.Headline = dto.Headline ?? user.Headline;
            user.Bio = dto.Bio ?? user.Bio;

            // If role update is allowed
            if (dto.Role is not null) user.Role = dto.Role.Value;
            await _userRepository.UpdateUserAsync(user);
            
            return true;
        }
        
        // Validators
        public async Task<bool> UserExistsAsyncByGuid(string guid)
        {
            return await _userRepository.UserExistsAsyncByGuid(guid);
        }
        public async Task<bool> UserExistsAsyncById(int id)
        {
            return await _userRepository.UserExistsAsyncById(id);
        }
    }
}