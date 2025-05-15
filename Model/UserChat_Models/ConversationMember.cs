using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMentor.Backend.Model.UserChat_Models;

// ConversationMember connects users to conversations
public class ConversationMember : AuditableEntity
{
    [Key] public int Id { get; set; }
    
    public int ConversationId { get; set; } // Foreign key to Conversation
    
    public int UserId { get; set; } // Foreign key to User
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public MemberRole Role { get; set; } = MemberRole.Regular;
    
    // Navigation properties
    [ForeignKey("ConversationId")] public Conversation? Conversation { get; set; }
    
    [ForeignKey("UserId")] public User? User { get; set; }
    
}
public enum MemberRole
{
    Admin,
    Regular
}