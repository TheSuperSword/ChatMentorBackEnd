namespace ChatMentor.Backend.DTOs;

public class KnowledgeBaseDtos
{
    public int Id { get; set; }
    public Guid KnowledgeBaseId{ get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public class KnowledgeBaseRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
}

public class KnowledgeSectionDtos
{
    public int Id { get; set; }
    public string Heading { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public KnowledgeSectionFileDto? File { get; set; }
}

public class KnowledgeSectionFileDto
{
    public Guid AttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class KnowledgeSectionRequest
{
    public string Heading { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public IFormFile File { get; set; }
}

