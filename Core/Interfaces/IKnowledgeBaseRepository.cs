using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Core.Interfaces;

public interface IKnowledgeBaseRepository
{
    // Create a Knowledge Base
    Task<KnowledgeBase> CreateKnowledgeBaseAsync(KnowledgeBase knowledgeBase);
    
    // Get all Knowledge Base
    Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBasesAsync();
    
    // Update Knowledge Base Info
    Task<KnowledgeBase?> UpdateKnowledgeBaseAsync(KnowledgeBase knowledgeBase);
    
    // Delete Knowledge Base
    Task<bool> DeleteKnowledgeBaseAsync(int id);
    
    // Create Knowledge Section
    Task<KnowledgeSection> CreateKnowledgeSectionAsync(KnowledgeSection section);
    
    // Get all Knowledge Section of Knowledge Base
    Task<IEnumerable<KnowledgeSection>> GetKnowledgeBaseSectionsAsync(int knowledgeBaseId);
    
    // Update a Knowledge Section
    Task<KnowledgeSection?> UpdateKnowledgeSectionAsync(KnowledgeSection section);
    
    // Delete a Knowledge Section  
    Task<bool> DeleteKnowledgeSectionAsync(int id);
}