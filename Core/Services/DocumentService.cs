using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Core.Services;

public class DocumentService(
    IDocumentRepository documentRepository,
    IWebHostEnvironment hostingEnvironment,
    IConfiguration configuration,
    ILogger<DocumentService> logger)
{
    private readonly IDocumentRepository _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    private readonly IWebHostEnvironment _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
    private readonly ILogger<DocumentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string? _uploadsBasePath = configuration["FileStorage:UploadsBasePath"];

    public async Task<Document?> UploadDocumentAsync(IFormFile file, Guid uploadedBy, string associatedEntity, Guid? relatedEntityId = null)
    {
        if (file is not { Length: > 0 }) throw new ArgumentException("Invalid file.");
        if (string.IsNullOrWhiteSpace(associatedEntity)) throw new ArgumentException("Associated entity cannot be null or empty.");

        // Ensure upload folder exists
        var uploadFolderPath = EnsureDirectoryExists(associatedEntity);
        var fileName = GenerateUniqueFilename(file.FileName);
        var filePath = Path.Combine(uploadFolderPath, fileName);
        // Save file locally
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save metadata to the database
        var document = new Document
        {
            DocId = Guid.NewGuid(),
            FileName = file.FileName,
            FilePath = GetRelativePath(filePath),
            ContentType = file.ContentType,
            FileSize = file.Length,
            CreatedBy = uploadedBy,
            CreatedAt = DateTime.UtcNow,
            AssociatedEntity = associatedEntity,
            RelatedEntityId = relatedEntityId
        };

        return await _documentRepository.UploadDocumentAsync(document);
    }

    public async Task<Document?> GetDocumentByGuidAsync(string guid)
    {
        if (!Guid.TryParse(guid, out _))
            throw new ArgumentException("Invalid GUID format.");

        return await _documentRepository.GetDocumentByGuidAsync(guid);
    }

    public async Task<bool> UpdateDocumentAsync(string docGuid, IFormFile newFile, Guid modifiedBy)
    {
        if (newFile == null || newFile.Length <= 0)
            throw new ArgumentException("Invalid replacement file.");

        var existingDocument = await _documentRepository.GetDocumentByGuidAsync(docGuid);
        if (existingDocument == null)
        {
            _logger.LogWarning("Document with GUID {DocGuid} not found for update.", docGuid);
            return false;
        }

        var uploadFolderPath = EnsureDirectoryExists(existingDocument.AssociatedEntity ?? "misc");
        var newFileName = GenerateUniqueFilename(newFile.FileName);
        var newFilePath = Path.Combine(uploadFolderPath, newFileName);

        try
        {
            // Save the new file
            await using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await newFile.CopyToAsync(stream);
            }

            // Delete the old file after successfully saving the new file
            var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath,
                existingDocument.FilePath.TrimStart('/'));
            if (File.Exists(oldFilePath)) File.Delete(oldFilePath);

            // Update metadata
            existingDocument.DocId = Guid.NewGuid(); // New file, new ID
            existingDocument.FileName = newFile.FileName;
            existingDocument.FilePath = GetRelativePath(newFilePath);
            existingDocument.ContentType = newFile.ContentType;
            existingDocument.FileSize = newFile.Length;
            existingDocument.UpdatedBy = modifiedBy;
            existingDocument.UpdatedAt = DateTime.UtcNow;

            return await _documentRepository.UpdateDocumentAsync(existingDocument);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating document with GUID {DocGuid}.", docGuid);
            throw;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string guid)
    {
        if (!Guid.TryParse(guid, out _))
            throw new ArgumentException("Invalid GUID format.");

        var document = await _documentRepository.GetDocumentByGuidAsync(guid);
        if (document == null)
        {
            _logger.LogWarning("Document with GUID {DocGuid} not found for deletion.", guid);
            return false;
        }

        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.FilePath.TrimStart('/'));
        try
        {
            // Delete file from disk
            if (File.Exists(filePath)) File.Delete(filePath);

            // Delete metadata from database
            return await _documentRepository.DeleteDocumentAsync(guid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting document with GUID {DocGuid}.", guid);
            throw;
        }
    }

    // Utility: Ensures directory exists, creates it if necessary
    private string EnsureDirectoryExists(string associatedEntity)
    {
        var folderPath = Path.Combine(_uploadsBasePath, associatedEntity.ToLowerInvariant());
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        return folderPath;
    }

    // Utility: Generates a unique file name with a GUID
    private string GenerateUniqueFilename(string originalFileName)
    {
        var safeName = Path.GetFileName(originalFileName); // Basic sanitization
        return $"{Guid.NewGuid()}_{safeName}";
    }

    // Utility: Returns a relative path from the full file path
    private string GetRelativePath(string fullPath)
    {
        return Path.GetRelativePath(_hostingEnvironment.WebRootPath, fullPath).Replace("\\", "/");
    }
}