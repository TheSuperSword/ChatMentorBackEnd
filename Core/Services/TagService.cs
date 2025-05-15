using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Repositories.Interfaces;

namespace ChatMentor.Backend.Services;

public class TagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
    {
        var tags = await _tagRepository.GetAllTagsAsync();
        return tags.Select(MapTagToDto);
    }

    public async Task<TagDto?> GetTagByIdAsync(int id)
    {
        var tag = await _tagRepository.GetTagByIdAsync(id);
        return tag != null ? MapTagToDto(tag) : null;
    }

    public async Task<TagDto?> GetTagByNameAsync(string name)
    {
        var tag = await _tagRepository.GetTagByNameAsync(name);
        return tag != null ? MapTagToDto(tag) : null;
    }

    public async Task<TagDto> CreateTagAsync(CreateTagDto createTagDto)
    {
        var tag = new Tag
        {
            Name = createTagDto.Name
        };

        var createdTag = await _tagRepository.CreateTagAsync(tag);
        return MapTagToDto(createdTag);
    }

    public async Task<TagDto?> UpdateTagAsync(int id, UpdateTagDto updateTagDto)
    {
        var tag = new Tag
        {
            Id = id,
            Name = updateTagDto.Name
        };

        var updatedTag = await _tagRepository.UpdateTagAsync(id, tag);
        return updatedTag != null ? MapTagToDto(updatedTag) : null;
    }

    public async Task<bool> DeleteTagAsync(int id)
    {
        return await _tagRepository.DeleteTagAsync(id);
    }

    public async Task<bool> TagExistsAsync(string name)
    {
        return await _tagRepository.TagExistsAsync(name);
    }

    public async Task<bool> TagExistsAsync(int id)
    {
        return await _tagRepository.TagExistsAsync(id);
    }

    private static TagDto MapTagToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}