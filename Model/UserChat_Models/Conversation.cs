using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Model.UserChat_Models;

// Conversation represents a chat between two or more users
[Index(nameof(ConversationId), IsUnique = true)]
public class Conversation : AuditableEntity
{
    [Key] public int Id { get; set; }
    
    [Required] public Guid ConversationId { get; set; } = Guid.NewGuid();
    
    [StringLength(100)] public string? Name { get; set; } // Optional name for group chats
    
    public bool IsGroup { get; set; } = false; // True if this is a group conversation
    
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public List<ConversationMember> Members { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
}