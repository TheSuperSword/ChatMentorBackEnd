using ChatMentor.Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatMentor.Backend.Core.Services;
using ChatMentor.Backend.Responses;
using ChatMentor.Backend.Services;
namespace ChatMentor.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserTagController : ControllerBase
{
    private readonly UserTagService _userTagService;
    private readonly UserService _userService;
    private readonly TagService _tagService;

    public UserTagController(
        UserTagService userTagService,
        UserService userService,
        TagService tagService)
    {
        _userTagService = userTagService;
        _userService = userService;
        _tagService = tagService;
    }

    // GET: api/UserTag
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserTagDto>>> GetUserTags()
    {
        var userTags = await _userTagService.GetAllUserTagsAsync();
        return Ok(JSendResponse<IEnumerable<UserTagDto>>.Success(userTags, "User tags retrieved successfully"));
    }

    // GET: api/UserTag/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserTagDto>> GetUserTag(int id)
    {
        var userTag = await _userTagService.GetUserTagByIdAsync(id);

        if (userTag == null)
        {
            return NotFound(JSendResponse<UserTagDto>.Fail(null, "User tag not found"));
        }

        return Ok(JSendResponse<UserTagDto>.Success(userTag, "User tag retrieved successfully"));
    }

    // GET: api/UserTag/user/5
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<UserTagsForUserDto>> GetTagsForUser(int userId)
    {
        if (!await _userService.UserExistsAsyncById(userId))
        {
            return NotFound(JSendResponse<UserTagsForUserDto>.Fail(null, "User not found"));
        }

        var tagsForUser = await _userTagService.GetTagsForUserAsync(userId);
        return Ok(JSendResponse<UserTagsForUserDto>.Success(tagsForUser, "Tags for user retrieved successfully"));
    }

    // GET: api/UserTag/tag/5
    [HttpGet("tag/{tagId}")]
    public async Task<ActionResult<UsersForTagDto>> GetUsersForTag(int tagId)
    {
        if (!await _tagService.TagExistsAsync(tagId))
        {
            return NotFound(JSendResponse<UsersForTagDto>.Fail(null, "Tag not found"));
        }

        var usersForTag = await _userTagService.GetUsersForTagAsync(tagId);
        return Ok(JSendResponse<UsersForTagDto>.Success(usersForTag, "Users for tag retrieved successfully"));
    }

    // POST: api/UserTag
    [HttpPost]
    [Authorize] // Add proper authorization as needed
    public async Task<ActionResult<UserTagDto>> AssignTagToUser(CreateUserTagDto createUserTagDto)
    {
        // Validate user exists
        if (!await _userService.UserExistsAsyncById(createUserTagDto.UserId))
        {
            return NotFound(JSendResponse<UserTagDto>.Fail(null, "User not found"));
        }

        // Validate tag exists
        if (!await _tagService.TagExistsAsync(createUserTagDto.TagId))
        {
            return NotFound(JSendResponse<UserTagDto>.Fail(null, "Tag not found"));
        }

        // Check if user already has this tag
        if (await _userTagService.UserTagExistsAsync(createUserTagDto.UserId, createUserTagDto.TagId))
        {
            return Conflict(JSendResponse<UserTagDto>.Fail(null, "User already has this tag assigned"));
        }

        var userTag = await _userTagService.AssignTagToUserAsync(createUserTagDto);
        return CreatedAtAction(nameof(GetUserTag), new { id = userTag.Id }, JSendResponse<UserTagDto>.Success(userTag, "Tag assigned to user successfully"));
    }

    // DELETE: api/UserTag/5
    [HttpDelete("{id}")]
    [Authorize] // Add proper authorization as needed
    public async Task<IActionResult> RemoveUserTag(int id)
    {
        if (!await _userTagService.UserTagExistsAsync(id))
        {
            return NotFound(JSendResponse<UserTagDto>.Fail(null, "User tag not found"));
        }

        var result = await _userTagService.RemoveTagFromUserAsync(id);
    
        if (!result)
        {
            return NotFound(JSendResponse<UserTagDto>.Fail(null, "User tag not found"));
        }

        return NoContent();
    }

    // DELETE: api/UserTag/user/5/tag/3
    [HttpDelete("user/{userId}/tag/{tagId}")]
    [Authorize] // Add proper authorization as needed
    public async Task<IActionResult> RemoveTagFromUser(int userId, int tagId)
    {
        if (!await _userTagService.UserTagExistsAsync(userId, tagId))
        {
            return NotFound(JSendResponse<UserTagDto>.Fail(null, "User does not have this tag assigned"));
        }

        var result = await _userTagService.RemoveTagFromUserAsync(userId, tagId);
    
        if (!result)
        {
            return NotFound(JSendResponse<UserTagDto>.Fail(null, "User tag not found"));
        }

        return NoContent();
    }
}