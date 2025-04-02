using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Core.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetDocumentByGuidAsync(string guid);
    Task<IEnumerable<Document>> GetDocumentsByEntityAsync(string? associatedEntity);
    Task<Document> UploadDocumentAsync(Document document);
    Task<bool> UpdateDocumentAsync(Document updatedDocument);
    Task<bool> DeleteDocumentAsync(string guid);
}