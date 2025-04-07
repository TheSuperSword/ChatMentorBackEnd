using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Model;
using System.Threading.Tasks;

namespace ChatMentor.Backend.Core.Services
{
    public class TagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IUserTagRepository _userTagRepository;

        public TagService(ITagRepository tagRepository, IUserTagRepository userTagRepository)
        {
            _tagRepository = tagRepository;
            _userTagRepository = userTagRepository;
        }

        // Add a new tag to the system
        public async Task AddTagAsync(string tagName)
        {
            // Check if tag already exists
            var existingTag = await _tagRepository.GetByNameAsync(tagName);
            if (existingTag != null) { throw new InvalidOperationException("Tag already exists."); }

            // Create a new tag and add it
            var newTag = new Tag { Name = tagName };
            await _tagRepository.AddAsync(newTag);
            await _tagRepository.SaveChangesAsync();
        }
        
        // Get all tags
        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllAsync();
        }

        // Get a tag by its ID
        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            return await _tagRepository.GetByIdAsync(id);
        }

        // Get a tag by its name
        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            return await _tagRepository.GetByNameAsync(name);
        }
        
        // Get the list of tags for a user
        public async Task<IEnumerable<Tag>> GetTagsByUserIdAsync(int userId)
        {
            // Fetch the UserTag entries for the given user
            var userTags = await _userTagRepository.GetAllByUserIdAsync(userId);

            // Convert UserTag list to Tag list by selecting the related Tag from UserTag
            var tags = userTags.Select(ut => ut.Tag).ToList();

            return tags;
        }


        // Remove a tag from a user
        public async Task RemoveTagFromUserAsync(int userId, int tagId)
        {
            var userTag = await _userTagRepository.GetByUserIdAndTagIdAsync(userId, tagId);
            if (userTag != null)
            {
                await _userTagRepository.Remove(userTag);
                await _userTagRepository.SaveChangesAsync();
            }
        }
        
        // Add a tag to a user
        public async Task AddTagToUserAsync(int userId, string tagName)
        {
            // Get the tag by name, create if it doesn't exist
            var tag = await _tagRepository.GetByNameAsync(tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                await _tagRepository.AddAsync(tag);
                await _tagRepository.SaveChangesAsync(); // Save tag first
            }

            // Create a user-tag relationship
            var userTag = new UserTag { UserId = userId, TagId = tag.Id };
            await _userTagRepository.AddAsync(userTag);
            await _userTagRepository.SaveChangesAsync();  // Save user-tag relationship
        }
    }
}
