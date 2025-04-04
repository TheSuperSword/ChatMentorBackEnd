using System.Security.Claims;
using ChatMentor.Backend.Model;
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