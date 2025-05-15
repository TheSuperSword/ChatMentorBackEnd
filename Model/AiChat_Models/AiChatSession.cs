using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Model;
[Table("AiChatSessions")]
[Index(nameof(SessionId), IsUnique = true)]
public class AiChatSession : AuditableEntity
{
    [Key]
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }  // Just a regular property now, no navigation

    [MaxLength(200)]
    public string? Title { get; set; }
    
    public SessionStatus SessionStatus { get; set; } = SessionStatus.Active;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime LastUpdatedAt { get; set; }

    // Only keeping the Chats navigation property
    public virtual ICollection<AIChat> Chats { get; set; } = new List<AIChat>();
}

public enum SessionStatus
{
    Active,
    Deleted,
    Error
}