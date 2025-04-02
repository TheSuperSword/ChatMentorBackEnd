using System.ComponentModel.DataAnnotations;
using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.DTOs;

public class UserDto
{
    //Temporary
    public int Id { get; set; }
    
    public string userGuid { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; }
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

