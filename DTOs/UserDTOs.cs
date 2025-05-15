using System.ComponentModel.DataAnnotations;
using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.DTOs;

public class UserDto
{
    public string UserGuid { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; }
    public List<TagDto> Tags { get; set; } = new();
}

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [EmailAddress] public string? Email { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    [EnumDataType(typeof(UserRole))] public UserRole? Role { get; set; }
}