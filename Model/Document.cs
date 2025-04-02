using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Model;

[Index(nameof(DocId), IsUnique = true)]
public class Document
{
    [Key] public int Id { get; set; } // Auto-incremented primary key (for easy querying)

    [Required] public Guid DocId { get; set; } // Unique identifier for the document (GUID)

    [Required] public string FileName { get; set; } = string.Empty; // Original file name

    [Required] public string FilePath { get; set; } = string.Empty; // Stored file path (relative)

    [Required] public string ContentType { get; set; } = string.Empty; // MIME type (image/png, application/pdf, etc.)

    public long FileSize { get; set; } // File size in bytes

    [Required] public Guid UploadedBy { get; set; } // User ID of uploader

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Guid? LastModifiedBy { get; set; } // User ID of the last modifier

    public DateTime? LastModifiedAt { get; set; } // Timestamp of the last modification
    // Timestamp

    public string? AssociatedEntity { get; set; } // "ProfilePic", "ChatAttachment", "Resource"

    public Guid? RelatedEntityId { get; set; } // Reference to a chat, user, or resource
}