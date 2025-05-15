using System.ComponentModel.DataAnnotations;

namespace ChatMentor.Backend.DTOs;

public class UserTagDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TagId { get; set; }
    public string? TagName { get; set; }
    public string? UserFullName { get; set; }
}

public class CreateUserTagDto
{
    [Required] public int UserId { get; set; }

    [Required] public int TagId { get; set; }
}

public class UserTagsForUserDto
{
    public int UserId { get; set; }
    public List<TagDto> Tags { get; set; } = new();
}

public class UsersForTagDto
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public List<UserDto> Users { get; set; } = new();
}