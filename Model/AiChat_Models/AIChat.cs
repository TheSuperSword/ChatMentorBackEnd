using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMentor.Backend.Model;

[Table("AIChats")]
public class AIChat
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Session")]
    public Guid SessionId { get; set; }
    
    [Required]
    public string UserMessage { get; set; }

    [Required]
    public string AIResponse { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    // Only keeping the Session navigation property
    public virtual AiChatSession Session { get; set; } = null!;
}