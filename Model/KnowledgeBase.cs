using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatMentor.Backend.Model;

public class KnowledgeBase : AuditableEntity
{
    [Key]
    public int Id { get; set; } // Auto-incremented primary key
    
    [Required]
    public Guid KnowledgeBaseId { get; set; } = Guid.NewGuid(); // Unique identifier (GUID)
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    // Navigation property for sections
    public ICollection<KnowledgeSection> Sections { get; set; } = new List<KnowledgeSection>();
}