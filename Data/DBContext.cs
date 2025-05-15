using System.Security.Claims;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Model.UserChat_Models;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Data;

public class ChatMentorDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ChatMentorDbContext(DbContextOptions<ChatMentorDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<AuditLog> TblAuditLogs { get; set; }
    public DbSet<User> TblUser { get; set; }
    public DbSet<Tag> TblTag { get; set; }
    public DbSet<UserTag> TblUserTag { get; set; }
    public DbSet<Document> TblDocument { get; set; }
    // Ai Chat
    public DbSet<AIChat> TblAiChats { get; set; }
    public DbSet<AiChatSession> TblAiChatSessions { get; set; }
    
    // User Chat
    public DbSet<Message> TblMessages { get; set; }
    public DbSet<Conversation> TblConversations { get; set; }
    public DbSet<ConversationMember> TblConversationMembers { get; set; }
    public DbSet<UserConnection> TblUserConnections { get; set; }
    public DbSet<MessageAttachment> TblMessageAttachments { get; set; }
    
    // Knowledge Base
    public DbSet<KnowledgeBase> TblKnowledgeBase { get; set; }
    public DbSet<KnowledgeSection> TblKnowledgeSections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserTag>()
            .HasOne(ut => ut.User)
            .WithMany(u => u.UserTags)
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTag>()
            .HasOne(ut => ut.Tag)
            .WithMany(t => t.UserTags)
            .HasForeignKey(ut => ut.TagId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<AIChat>()
            .HasOne(c => c.Session)
            .WithMany(s => s.Chats)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        // Unique constraint on Conversation.ConversationId
        modelBuilder.Entity<Conversation>()
            .HasIndex(c => c.ConversationId)
            .IsUnique();

        // Optional: Enforce unique members per conversation
        modelBuilder.Entity<ConversationMember>()
            .HasIndex(cm => new { cm.ConversationId, cm.UserId })
            .IsUnique();

        // Configure relationships and cascade rules
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.ReplyToMessage)
            .WithMany()
            .HasForeignKey(m => m.ReplyToMessageId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ConversationMember>()
            .HasOne(cm => cm.Conversation)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConversationMember>()
            .HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserConnection>()
            .HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.SentMessages)
            .WithOne(m => m.Sender)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Conversations)
            .WithOne(cm => cm.User)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Connections)
            .WithOne(uc => uc.User)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<MessageAttachment>()
            .HasOne(ma => ma.Message)
            .WithMany()
            .HasForeignKey(ma => ma.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<MessageAttachment>()
            .HasOne(ma => ma.Document)
            .WithMany()
            .HasForeignKey(ma => ma.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<KnowledgeBase>()
            .HasMany(kb => kb.Sections)
            .WithOne(ks => ks.KnowledgeBase)
            .HasForeignKey(ks => ks.KnowledgeBaseId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<KnowledgeBase>()
            .HasIndex(kb => kb.KnowledgeBaseId)
            .IsUnique();
        
        modelBuilder.Entity<KnowledgeSection>()
            .HasOne(ks => ks.Document)
            .WithMany()
            .HasForeignKey(ks => ks.DocumentId)
            .OnDelete(DeleteBehavior.Restrict); 
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries<AuditableEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        var userId = GetCurrentUserId();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = userId;
            }

            entry.Entity.UpdatedAt = DateTime.UtcNow;
            entry.Entity.UpdatedBy = userId;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private Guid? GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userId, out var guid) ? guid : new Guid("00000000-0000-0000-0000-000000000000");
    }
}