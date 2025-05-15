using System.Security.Claims;
using ChatMentor.Backend.Core.Services;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatMentor.Backend.Controllers;

[ApiController]
[Route("api/knowledgebase")]
public class KnowledgeBaseController : ControllerBase
{
    private readonly KnowledgeBaseService _knowledgeBaseService;
    private readonly DocumentService _documentService;


    public KnowledgeBaseController(KnowledgeBaseService knowledgeBaseService, DocumentService documentService)
    {
        _knowledgeBaseService = knowledgeBaseService;
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllKnowledgeBases()
    {
        var knowledgeBases = await _knowledgeBaseService.GetAllKnowledgeBasesAsync();
        return Ok(JSendResponse<IEnumerable<KnowledgeBaseDtos>>.Success(knowledgeBases));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateKnowledgeBase([FromBody] KnowledgeBaseRequest request)
    {
        if (request == null)
            return BadRequest(JSendResponse<object>.Fail(null, "Invalid request data."));

        var createdKnowledgeBase = await _knowledgeBaseService.CreateKnowledgeBase(request);
        if (createdKnowledgeBase == null)
            return BadRequest(JSendResponse<object>.Fail(null, "Failed to create knowledge base."));

        return CreatedAtAction(nameof(GetAllKnowledgeBases), 
            new { id = createdKnowledgeBase.Id }, 
            JSendResponse<KnowledgeBaseDtos>.Success(createdKnowledgeBase));
    }

    [HttpPost("{knowledgeBaseId}/sections")]
    [Authorize]
    public async Task<IActionResult> CreateKnowledgeSection(int knowledgeBaseId, [FromForm] KnowledgeSectionRequest request)
    {
        if (request == null)
            return BadRequest(JSendResponse<object>.Fail(null, "Invalid request data."));

        var currentUserGuid = GetUserIdFromClaims();
        var createdSection = await _knowledgeBaseService.CreateKnowledgeSection(request, currentUserGuid, knowledgeBaseId);

        if (createdSection == null)
            return BadRequest(JSendResponse<object>.Fail(null, "Failed to create knowledge section."));

        // FIX: Corrected parameter name to match the GetKnowledgeSectionsById method signature
        return CreatedAtAction(nameof(GetKnowledgeSectionsById), 
            new { knowledgeBaseId = knowledgeBaseId }, 
            JSendResponse<KnowledgeSectionDtos>.Success(createdSection));
    }

    [HttpGet("{knowledgeBaseId}/sections")]
    public async Task<IActionResult> GetKnowledgeSectionsById(int knowledgeBaseId)
    {
        var sections = await _knowledgeBaseService.GetKnowledgeSectionByIdAsync(knowledgeBaseId);
        return Ok(JSendResponse<IEnumerable<KnowledgeSectionDtos>>.Success(sections, "Successfully retrieved knowledge base sections"));
    }
    
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadDocument(string id)
    {
        try
        {
            var (filePath, fileName, contentType) = await _documentService.PrepareFileForDownloadAsync(id);
        
            // For security, use PhysicalFileResult to prevent path traversal attacks
            return PhysicalFile(filePath, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while downloading the document.");
        }
    }
    
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User not authenticated or invalid user ID");
        }
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID format");
        }
        return userId;
    }
}