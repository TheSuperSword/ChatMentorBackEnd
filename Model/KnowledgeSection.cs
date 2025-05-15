using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMentor.Backend.Model;

public class KnowledgeSection : AuditableEntity
{
    [Key]
    public int Id { get; set; } // Auto-incremented primary key
    
    [Required]
    public string Heading { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    // Content extracted or entered for this section
    public string? Content { get; set; }
    
    // Foreign key to Document
    [Required]
    public int DocumentId { get; set; }
    
    [ForeignKey("DocumentId")]
    public Document Document { get; set; } = null!;
    
    // Foreign key to KnowledgeBase
    [Required]
    public int KnowledgeBaseId { get; set; }
    
    [ForeignKey("KnowledgeBaseId")]
    public KnowledgeBase KnowledgeBase { get; set; } = null!;
}