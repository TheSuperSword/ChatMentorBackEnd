using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Responses;
using ChatMentor.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatMentor.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly TagService _tagService;

    public TagController(TagService tagService)
    {
        _tagService = tagService;
    }

    // GET: api/Tag
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return Ok(JSendResponse<IEnumerable<TagDto>>.Success(tags, "Tags retrieved successfully"));
    }

    // GET: api/Tag/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);

        if (tag == null)
        {
            return NotFound(JSendResponse<TagDto>.Fail(null, "Tag not found"));
        }

        return Ok(JSendResponse<TagDto>.Success(tag, "Tag retrieved successfully"));
    }

    // GET: api/Tag/name/Software
    [HttpGet("name/{name}")]
    public async Task<ActionResult<TagDto>> GetTagByName(string name)
    {
        var tag = await _tagService.GetTagByNameAsync(name);

        if (tag == null)
        {
            return NotFound(JSendResponse<TagDto>.Fail(null, "Tag not found"));
        }

        return Ok(JSendResponse<TagDto>.Success(tag, "Tag retrieved successfully"));
    }

    // POST: api/Tag
    [HttpPost]
    [Authorize] // Add proper authorization as needed
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto)
    {
        if (await _tagService.TagExistsAsync(createTagDto.Name))
        {
            return Conflict(JSendResponse<TagDto>.Fail(null, "Tag already exists"));
        }
    
        var createdTag = await _tagService.CreateTagAsync(createTagDto);
        return CreatedAtAction(nameof(GetTag), new { id = createdTag.Id }, JSendResponse<TagDto>.Success(createdTag, "Tag created successfully"));
    }
    
    // PUT: api/Tag/5
    [HttpPut("{id}")]
    [Authorize] // Add proper authorization as needed
    public async Task<IActionResult> UpdateTag(int id, UpdateTagDto updateTagDto)
    {
        if (!await _tagService.TagExistsAsync(id))
        {
            return NotFound(JSendResponse<TagDto>.Fail(null, "Tag not found"));
        }
    
        // Check if the new name conflicts with an existing tag (excluding the current tag)
        var existingTag = await _tagService.GetTagByNameAsync(updateTagDto.Name);
        if (existingTag != null && existingTag.Id != id)
        {
            return Conflict(JSendResponse<TagDto>.Fail(null, "A tag with this name already exists"));
        }
    
        var updatedTag = await _tagService.UpdateTagAsync(id, updateTagDto);
        
        if (updatedTag == null)
        {
            return NotFound(JSendResponse<TagDto>.Fail(null, "Tag not found"));
        }
    
        return Ok(JSendResponse<TagDto>.Success(updatedTag, "Tag updated successfully"));
    }
    
    // DELETE: api/Tag/5
    [HttpDelete("{id}")]
    [Authorize] // Add proper authorization as needed
    public async Task<IActionResult> DeleteTag(int id)
    {
        if (!await _tagService.TagExistsAsync(id))
        {
            return NotFound(JSendResponse<TagDto>.Fail(null, "Tag not found"));
        }
    
        var result = await _tagService.DeleteTagAsync(id);
        
        if (!result)
        {
            return NotFound(JSendResponse<TagDto>.Fail(null, "Tag not found"));
        }
    
        return NoContent();
    }
}