namespace ChatMentor.Backend.Model;

public class AuditLog
{
    public int Id { get; set; } // Primary key
    public Guid? UserId { get; set; } // Can store the GUID of the user
    public string IpAddress { get; set; }
    public string Method { get; set; } // HTTP Method (GET, POST, etc.)
    public string Path { get; set; } // Path (e.g., /api/register)
    public string QueryString { get; set; }
    public string RequestBody { get; set; } // Optional, depending on your needs
    public int StatusCode { get; set; } // Response Status Code
    public DateTime CreatedAt { get; set; }
}