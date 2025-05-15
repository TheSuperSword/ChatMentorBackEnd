using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories;

public class KnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly ChatMentorDbContext _context;

    public KnowledgeBaseRepository(ChatMentorDbContext context)
    {
        _context = context;
    }

    public async Task<KnowledgeBase> CreateKnowledgeBaseAsync(KnowledgeBase knowledgeBase)
    {
        await _context.TblKnowledgeBase.AddAsync(knowledgeBase);
        await _context.SaveChangesAsync();
        return knowledgeBase;
    }

    public async Task<IEnumerable<KnowledgeBase>> GetAllKnowledgeBasesAsync()
    {
        return await _context.TblKnowledgeBase
            .Include(kb => kb.Sections)
            .ToListAsync();
    }

    public async Task<KnowledgeBase?> UpdateKnowledgeBaseAsync(KnowledgeBase knowledgeBase)
    {
        var existingKb = await _context.TblKnowledgeBase.FindAsync(knowledgeBase.Id);
        if (existingKb == null) return null;
        _context.Entry(existingKb).CurrentValues.SetValues(knowledgeBase);
        await _context.SaveChangesAsync();
        return existingKb;
    }

    public async Task<bool> DeleteKnowledgeBaseAsync(int id)
    {
        var knowledgeBase = await _context.TblKnowledgeBase.FindAsync(id);
        if (knowledgeBase == null)
            return false;

        _context.TblKnowledgeBase.Remove(knowledgeBase);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<KnowledgeSection> CreateKnowledgeSectionAsync(KnowledgeSection section)
    {
        await _context.TblKnowledgeSections.AddAsync(section);
        await _context.SaveChangesAsync();
        return section;
    }

    public async Task<IEnumerable<KnowledgeSection>> GetKnowledgeBaseSectionsAsync(int knowledgeBaseId)
    {
        return await _context.TblKnowledgeSections
            .Include(ks => ks.Document)
            .Where(ks => ks.KnowledgeBaseId == knowledgeBaseId)
            .ToListAsync();
    }

    public async Task<KnowledgeSection?> UpdateKnowledgeSectionAsync(KnowledgeSection section)
    {
        var existingSection = await _context.TblKnowledgeSections.FindAsync(section.Id);
        if (existingSection == null) return null;
        _context.Entry(existingSection).CurrentValues.SetValues(section);
        await _context.SaveChangesAsync();
        return existingSection;
    }

    public async Task<bool> DeleteKnowledgeSectionAsync(int id)
    {
        var section = await _context.TblKnowledgeSections.FindAsync(id);
        if (section == null) return false;
        _context.TblKnowledgeSections.Remove(section);
        await _context.SaveChangesAsync();
        return true;
    }
}