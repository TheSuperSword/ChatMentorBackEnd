using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Core.Services;

public class DocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly string _documentsPath;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<DocumentService> _logger;
    private readonly string _profilePicturesPath;
    private readonly string _uploadsBasePath;

    public DocumentService(
        IDocumentRepository documentRepository,
        IWebHostEnvironment hostingEnvironment,
        IConfiguration configuration,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Get paths from configuration with fallbacks
        _uploadsBasePath = configuration["FileStorage:UploadsBasePath"];
        if (string.IsNullOrEmpty(_uploadsBasePath))
        {
            _uploadsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            _logger.LogWarning("FileStorage:UploadsBasePath not configured. Using default path: {Path}",
                _uploadsBasePath);
        }

        _profilePicturesPath = configuration["FileStorage:ProfilePicturesPath"] ?? "/profile_pics";
        _documentsPath = configuration["FileStorage:DocumentsPath"] ?? "/documents";

        // Ensure the base uploads directory exists
        if (!Directory.Exists(_uploadsBasePath))
        {
            Directory.CreateDirectory(_uploadsBasePath);
            _logger.LogInformation("Created base uploads directory: {Path}", _uploadsBasePath);
        }

        // Ensure subdirectories exist
        EnsureDirectoryExists("profile_pics");
        EnsureDirectoryExists("documents");
        EnsureDirectoryExists("chats");
        EnsureDirectoryExists("knowledge_base");
    }

    public async Task<Document?> UploadDocumentAsync(IFormFile file, Guid uploadedBy, string associatedEntity, Guid? relatedEntityId = null)
    {
        if (file is not { Length: > 0 }) throw new ArgumentException("Invalid file.");
        if (string.IsNullOrWhiteSpace(associatedEntity)) throw new ArgumentException("Associated entity cannot be null or empty.");

        // Ensure upload folder exists
        var uploadFolderPath = EnsureDirectoryExists(associatedEntity);
        var fileName = GenerateUniqueFilename(file.FileName);
        var filePath = Path.Combine(uploadFolderPath, fileName);

        try
        {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} for entity {Entity}", file.FileName,
                associatedEntity);

            // Clean up the file if it was created but database operation failed
            if (File.Exists(filePath))
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Log but don't throw to avoid masking the original exception
                    _logger.LogWarning("Failed to delete file {FilePath} after upload error", filePath);
                }

            throw;
        }
    }
    
    public async Task<(string filePath, string fileName, string contentType)> PrepareFileForDownloadAsync(string docGuid)
    {
        if (!Guid.TryParse(docGuid, out _))
            throw new ArgumentException("Invalid GUID format.");

        var document = await _documentRepository.GetDocumentByGuidAsync(docGuid);
        if (document == null)
        {
            _logger.LogWarning("Document with GUID {DocGuid} not found for download.", docGuid);
            throw new FileNotFoundException($"Document with ID {docGuid} not found.");
        }

        try
        {
            // Resolve the file path
            if (string.IsNullOrEmpty(document.FilePath))
                throw new InvalidOperationException($"Document with ID {docGuid} has no associated file path.");

            var absoluteFilePath = ResolveAbsolutePath(document.FilePath);
        
            if (!File.Exists(absoluteFilePath))
            {
                _logger.LogWarning("File for document {DocGuid} not found at path {FilePath}", docGuid, absoluteFilePath);
                throw new FileNotFoundException($"File for document with ID {docGuid} not found.");
            }

            // Return the file information needed for download
            return (absoluteFilePath, document.FileName, document.ContentType);
        }
        catch (Exception ex) when (ex is not FileNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "An error occurred while preparing document with GUID {DocGuid} for download.", docGuid);
            throw;
        }
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
        string? oldFilePath = null;

        try
        {
            // Save the new file
            await using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await newFile.CopyToAsync(stream);
            }

            // Get the old file path - handle potential null/invalid paths gracefully
            if (!string.IsNullOrEmpty(existingDocument.FilePath))
                try
                {
                    oldFilePath = ResolveAbsolutePath(existingDocument.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not resolve absolute path for document {DocGuid}", docGuid);
                    // Continue with the update even if we can't delete the old file
                }

            // Update metadata
            existingDocument.FileName = newFile.FileName;
            existingDocument.FilePath = GetRelativePath(newFilePath);
            existingDocument.ContentType = newFile.ContentType;
            existingDocument.FileSize = newFile.Length;
            existingDocument.UpdatedBy = modifiedBy;
            existingDocument.UpdatedAt = DateTime.UtcNow;

            var result = await _documentRepository.UpdateDocumentAsync(existingDocument);

            // Delete the old file after successfully saving the new file and updating the DB
            if (result && oldFilePath != null && File.Exists(oldFilePath))
                try
                {
                    File.Delete(oldFilePath);
                }
                catch (Exception ex)
                {
                    // Log but continue - the update was successful
                    _logger.LogWarning(ex, "Failed to delete old file {FilePath} after successful update", oldFilePath);
                }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating document with GUID {DocGuid}.", docGuid);

            // Clean up the new file if it was created but update failed
            if (File.Exists(newFilePath))
                try
                {
                    File.Delete(newFilePath);
                }
                catch
                {
                    // Log but don't throw to avoid masking the original exception
                    _logger.LogWarning("Failed to delete file {FilePath} after update error", newFilePath);
                }

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

        try
        {
            // Delete file from disk if path is valid
            if (!string.IsNullOrEmpty(document.FilePath))
                try
                {
                    var filePath = ResolveAbsolutePath(document.FilePath);
                    if (File.Exists(filePath)) File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Log but continue with database deletion
                    _logger.LogWarning(ex, "Error deleting file for document {DocGuid}", guid);
                }

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
        // Standardize the entity name
        var entityDir = associatedEntity.ToLowerInvariant();

        // Choose the appropriate base directory based on entity type
        string subPath;
        if (entityDir == "profilepictures" || entityDir == "profile_pictures" || entityDir == "profile")
            subPath = _profilePicturesPath.TrimStart('/');
        else if (entityDir == "documents" || entityDir == "docs")
            subPath = _documentsPath.TrimStart('/');
        else
            // For anything else, use a subdirectory with the entity name
            subPath = entityDir;

        var folderPath = Path.Combine(_uploadsBasePath, subPath);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            _logger.LogInformation("Created directory for entity {Entity}: {Path}", associatedEntity, folderPath);
        }

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
        if (string.IsNullOrEmpty(_uploadsBasePath))
            throw new InvalidOperationException("Uploads base path is not configured properly.");

        try
        {
            return Path.GetRelativePath(_uploadsBasePath, fullPath).Replace("\\", "/");
        }
        catch (ArgumentNullException)
        {
            _logger.LogError("Failed to get relative path. Base path: '{BasePath}', File path: '{FilePath}'",
                _uploadsBasePath, fullPath);
            throw new InvalidOperationException(
                "Could not calculate relative path. Check that all paths are properly configured.");
        }
    }

    // Utility: Resolves a relative path to an absolute path
    private string ResolveAbsolutePath(string relativePath)
    {
        if (string.IsNullOrEmpty(_uploadsBasePath))
            throw new InvalidOperationException("Uploads base path is not configured properly.");

        // If the path already starts with the upload base path, it's already absolute
        if (relativePath.StartsWith(_uploadsBasePath, StringComparison.OrdinalIgnoreCase)) return relativePath;

        // Handle both types of slashes and trim leading slash if present
        relativePath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString())
            .TrimStart(Path.DirectorySeparatorChar);
        return Path.Combine(_uploadsBasePath, relativePath);
    }
}