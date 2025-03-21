using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Model
{
    [Index(nameof(UserId), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }  // Auto-incremented primary key (for easy querying)

        [Required]
        public Guid UserId { get; set; } = Guid.NewGuid();  

        // User Information
        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; } 
        
        [Required]
        [StringLength(50)]
        public required string LastName { get; set; } 
        
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public required string Email { get; set; }  // User's email

        [StringLength(50)]
        public string? Headline { get; set; }  // User's title or headline

        [StringLength(500)]
        public string? Bio { get; set; }  // Short user bio
        
        // Authentication & Security
        [Required]
        [StringLength(256)]
        public required string PasswordHash { get; set; }  // Hashed password

        [Required]
        [StringLength(50)]
        public required string Role { get; set; }  // "Mentor" or "Student"

        public int FailedLoginAttempts { get; set; } = 0;  // Number of failed attempts

        public AccountStatus Status { get; set; } = AccountStatus.Active;  // Using enum instead of string

        public DateTime? LastLogon { get; set; }  // Last login timestamp

        [StringLength(45)]
        public string? LastLogonIp { get; set; }  // IP Address of last login

        public DateTime? PasswordChangedAt { get; set; }  // Last password change

        // Audit Fields
        [StringLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property for Many-to-Many Tags
        
        public List<UserTag> UserTags { get; set; } = [];
    }

    public enum AccountStatus
    {
        Active,
        Suspended,
        Banned
    }
}
