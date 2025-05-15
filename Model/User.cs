using System.ComponentModel.DataAnnotations;
using ChatMentor.Backend.Model.UserChat_Models;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Model;

[Index(nameof(UserId), IsUnique = true)]
public class User : AuditableEntity
{
    [Key] public int Id { get; set; }

    [Required] public Guid UserId { get; set; } = Guid.NewGuid();

    // User Information
    [Required] [StringLength(50)] public required string FirstName { get; set; }
    [Required] [StringLength(50)] public required string LastName { get; set; }
    [Required] [EmailAddress] [StringLength(256)] public required string Email { get; set; }
    [StringLength(50)] public string? Headline { get; set; }
    [StringLength(500)] public string? Bio { get; set; }
    [StringLength(500)] public string? ProfilePictureUrl { get; set; }

    // Authentication & Security
    [Required] [StringLength(256)] public required string PasswordHash { get; set; }
    [Required] public UserRole Role { get; set; } = UserRole.Student;
    public int FailedLoginAttempts { get; set; } = 0;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public DateTime? LastLogon { get; set; }
    [StringLength(45)] public string? LastLogonIp { get; set; }
    public DateTime? PasswordChangedAt { get; set; }

    // Refresh Token Properties
    [StringLength(512)] public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

    // Navigation Properties
    public List<UserTag> UserTags { get; set; } = [];
    public List<Message> SentMessages { get; set; } = [];
    public List<ConversationMember> Conversations { get; set; } = [];
    public List<UserConnection> Connections { get; set; } = [];
}

public enum AccountStatus
{
    Active,
    Suspended,
    Banned
}

public enum UserRole
{
    Admin,
    Student,
    Mentor,
    Guest
}