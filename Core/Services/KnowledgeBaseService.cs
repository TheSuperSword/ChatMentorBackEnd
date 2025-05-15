using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model;
using Microsoft.AspNetCore.Http;

namespace ChatMentor.Backend.Core.Services;

public class KnowledgeBaseService
{
    private readonly IKnowledgeBaseRepository _knowledgeBaseRepository;
    private readonly DocumentService _documentService;
    private readonly IFileConverterService _fileConverterService;

    public KnowledgeBaseService(
        IKnowledgeBaseRepository knowledgeBaseRepository, 
        DocumentService documentService,
        IFileConverterService fileConverterService)
    {
        _knowledgeBaseRepository = knowledgeBaseRepository;
        _documentService = documentService;
        _fileConverterService = fileConverterService;
    }

    public async Task<IEnumerable<KnowledgeBaseDtos>> GetAllKnowledgeBasesAsync()
    {
        var kbDtos = new List<KnowledgeBaseDtos>();
        var kbList =  await _knowledgeBaseRepository.GetAllKnowledgeBasesAsync();
        foreach (var kb in kbList)  kbDtos.Add(await KBMapToDtoAsync(kb));
        return kbDtos;
    }

    public async Task<KnowledgeBaseDtos?> CreateKnowledgeBase(KnowledgeBaseRequest dto)
    {
        var kb = new KnowledgeBase {
            KnowledgeBaseId = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
        };
        var createdKb = await _knowledgeBaseRepository.CreateKnowledgeBaseAsync(kb);
        return await KBMapToDtoAsync(createdKb);
    }

    public async Task<KnowledgeSectionDtos> CreateKnowledgeSection(KnowledgeSectionRequest dto, Guid currentUserGuid, int knowledgeBaseId)
    {
        // Convert the uploaded file to PDF
        IFormFile pdfFile = await _fileConverterService.ConvertToPdfAsync(dto.File);
        
        // Upload the converted PDF file
        var document = await _documentService.UploadDocumentAsync(pdfFile, currentUserGuid, "knowledge_base");
        
        var knowledgeSection = new KnowledgeSection { 
            Heading = dto.Heading,
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            DocumentId = document.Id,
            KnowledgeBaseId = knowledgeBaseId, 
        }; 
        var createdKs = await _knowledgeBaseRepository.CreateKnowledgeSectionAsync(knowledgeSection);
        var ksFile = new KnowledgeSectionFileDto
        {
            AttachmentId = document.DocId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            DownloadUrl = $"To be implemented {document.DocId}"
        };
        var ksDtos = new KnowledgeSectionDtos
        {
            Id = createdKs.Id,
            Heading = createdKs.Heading,
            Title = createdKs.Title,
            Description = createdKs.Description,
            Content = createdKs.Content,
            File = ksFile
        };
        return ksDtos;
    }

    public async Task<List<KnowledgeSectionDtos>> GetKnowledgeSectionByIdAsync(int id)
    {
        var fetched = await _knowledgeBaseRepository.GetKnowledgeBaseSectionsAsync(id);
        var ksDtos = new List<KnowledgeSectionDtos>();
        foreach (var ks in fetched) ksDtos.Add(await KSMapToDtoAsync(ks));
        return ksDtos;
    }
    
    private async Task<KnowledgeBaseDtos> KBMapToDtoAsync(KnowledgeBase kb)
    {
        return new KnowledgeBaseDtos
        {
            Id = kb.Id,
            KnowledgeBaseId = kb.KnowledgeBaseId,
            Title = kb.Title,
            Description = kb.Description
        };
    }

    private async Task<KnowledgeSectionDtos> KSMapToDtoAsync(KnowledgeSection ks)
    {
        var ksFile = new KnowledgeSectionFileDto
        {
            AttachmentId = ks.Document.DocId,
            FileName = ks.Document.FileName,
            ContentType = ks.Document.ContentType,
            FileSize = ks.Document.FileSize,
            DownloadUrl = $"To be implemented {ks.Document.DocId}"
        };
        return new KnowledgeSectionDtos
        {
            Id = ks.Id,
            Heading = ks.Heading,
            Title = ks.Title,
            Description = ks.Description,
            Content = ks.Content,
            File = ksFile
        };
    }
}