using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.DbContext;
using ChatMentor.Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ChatMentorDbContext _context;

        public DocumentRepository(ChatMentorDbContext context)
        {
            _context = context;
        }

        public async Task<Document?> GetDocumentByGuidAsync(string guid)
        {
            return await _context.TblDocument.FirstOrDefaultAsync(d => d.DocId == Guid.Parse(guid));
        }

        public async Task<IEnumerable<Document>> GetDocumentsByEntityAsync(string? associatedEntity)
        {
            if (string.IsNullOrWhiteSpace(associatedEntity))
                return await _context.TblDocument.ToListAsync();

            return await _context.TblDocument
                .Where(d => d.AssociatedEntity == associatedEntity)
                .ToListAsync();
        }

        public async Task<Document> UploadDocumentAsync(Document document)
        {
            _context.TblDocument.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<bool> UpdateDocumentAsync(Document updatedDocument)
        {
            var existingDocument = await _context.TblDocument.FirstOrDefaultAsync(d => d.DocId == updatedDocument.DocId);
            if (existingDocument == null)
                return false;

            existingDocument.FileName = updatedDocument.FileName;
            existingDocument.FilePath = updatedDocument.FilePath;
            existingDocument.LastModifiedBy = updatedDocument.LastModifiedBy;
            existingDocument.LastModifiedAt = updatedDocument.LastModifiedAt;

            _context.TblDocument.Update(existingDocument);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDocumentAsync(string guid)
        {
            var document = await _context.TblDocument.FirstOrDefaultAsync(d => d.DocId == Guid.Parse(guid));
            if (document == null)
                return false;

            _context.TblDocument.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
