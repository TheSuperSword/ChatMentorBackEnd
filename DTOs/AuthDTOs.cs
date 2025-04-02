using System.ComponentModel.DataAnnotations;
using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.DTOs;

public class RegisterUserDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    [EmailAddress] public required string Email { get; set; }
    public required string Headline { get; set; }
    public required string Bio { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    [EnumDataType(typeof(UserRole))] public UserRole? Role { get; set; }
}
public class LoginResponseDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? Headline { get; set; } // Optional headline
    public string? Bio { get; set; } // Optional bio
    public string? ProfilePictureUrl { get; set; } // Profile picture URL
    public string? Token { get; set; } // JWT Token for authentication
}
public class LoginUserDto
{
    [EmailAddress] public required string Email { get; set; }
    public required string Password { get; set; }
}