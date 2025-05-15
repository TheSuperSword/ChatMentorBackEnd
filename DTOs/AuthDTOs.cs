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
    public string UserGuid { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public UserRole Role { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public string? AccessToken { get; set; } // Changed from Token to AccessToken
    public string? RefreshToken { get; set; } // Added for refresh token flow
    public DateTime  RefreshTokenExpiresAt { get; set; } 
}

public class LoginUserDto
{
    [EmailAddress] public required string Email { get; set; }
    public required string Password { get; set; }
}

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime  RefreshTokenExpiresAt { get; set; }
}

public class RefreshTokenRequest
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}