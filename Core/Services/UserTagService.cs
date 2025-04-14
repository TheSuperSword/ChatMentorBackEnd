using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Repositories.Interfaces;

namespace ChatMentor.Backend.Services;

public class UserTagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserTagRepository _userTagRepository;

    public UserTagService(
        IUserTagRepository userTagRepository,
        IUserRepository userRepository,
        ITagRepository tagRepository)
    {
        _userTagRepository = userTagRepository;
        _userRepository = userRepository;
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<UserTagDto>> GetAllUserTagsAsync()
    {
        var userTags = await _userTagRepository.GetAllUserTagsAsync();
        return userTags.Select(MapUserTagToDto);
    }

    public async Task<UserTagDto?> GetUserTagByIdAsync(int id)
    {
        var userTag = await _userTagRepository.GetUserTagByIdAsync(id);
        return userTag != null ? MapUserTagToDto(userTag) : null;
    }

    public async Task<UserTagsForUserDto> GetTagsForUserAsync(int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return new UserTagsForUserDto { UserId = userId };

        var userTags = await _userTagRepository.GetUserTagsByUserIdAsync(userId);

        var result = new UserTagsForUserDto
        {
            UserId = userId,
            Tags = userTags
                .Where(ut => ut.Tag != null)
                .Select(ut => new TagDto
                {
                    Id = ut.TagId,
                    Name = ut.Tag!.Name
                })
                .ToList()
        };

        return result;
    }

    public async Task<UsersForTagDto> GetUsersForTagAsync(int tagId)
    {
        var tag = await _tagRepository.GetTagByIdAsync(tagId);
        if (tag == null) return new UsersForTagDto { TagId = tagId };

        var userTags = await _userTagRepository.GetUserTagsByTagIdAsync(tagId);

        var result = new UsersForTagDto
        {
            TagId = tagId,
            TagName = tag.Name,
            Users = userTags
                .Where(ut => ut.User != null)
                .Select(ut => new UserDto
                {
                    Id = ut.UserId,
                    UserGuid = ut.User!.UserId.ToString(),
                    FirstName = ut.User.FirstName,
                    LastName = ut.User.LastName,
                    Email = ut.User.Email,
                    ProfilePictureUrl = ut.User.ProfilePictureUrl,
                    Headline = ut.User.Headline,
                    Bio = ut.User.Bio,
                    Role = ut.User.Role
                })
                .ToList()
        };

        return result;
    }

    public async Task<UserTagDto?> GetUserTagByUserIdAndTagIdAsync(int userId, int tagId)
    {
        var userTag = await _userTagRepository.GetUserTagByUserIdAndTagIdAsync(userId, tagId);
        return userTag != null ? MapUserTagToDto(userTag) : null;
    }

    public async Task<UserTagDto> AssignTagToUserAsync(CreateUserTagDto createUserTagDto)
    {
        var userTag = new UserTag
        {
            UserId = createUserTagDto.UserId,
            TagId = createUserTagDto.TagId
        };

        var createdUserTag = await _userTagRepository.CreateUserTagAsync(userTag);

        // Fetch the related entities to include the names in the response
        var tag = await _tagRepository.GetTagByIdAsync(createUserTagDto.TagId);
        var user = await _userRepository.GetUserByIdAsync(createUserTagDto.UserId);

        return new UserTagDto
        {
            Id = createdUserTag.Id,
            UserId = createdUserTag.UserId,
            TagId = createdUserTag.TagId,
            TagName = tag?.Name,
            UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : null
        };
    }

    public async Task<bool> RemoveTagFromUserAsync(int id)
    {
        return await _userTagRepository.DeleteUserTagAsync(id);
    }

    public async Task<bool> RemoveTagFromUserAsync(int userId, int tagId)
    {
        return await _userTagRepository.DeleteUserTagAsync(userId, tagId);
    }

    public async Task<bool> UserTagExistsAsync(int id)
    {
        return await _userTagRepository.UserTagExistsAsync(id);
    }

    public async Task<bool> UserTagExistsAsync(int userId, int tagId)
    {
        return await _userTagRepository.UserTagExistsAsync(userId, tagId);
    }

    private static UserTagDto MapUserTagToDto(UserTag userTag)
    {
        return new UserTagDto
        {
            Id = userTag.Id,
            UserId = userTag.UserId,
            TagId = userTag.TagId,
            TagName = userTag.Tag?.Name,
            UserFullName = userTag.User != null ? $"{userTag.User.FirstName} {userTag.User.LastName}" : null
        };
    }
}