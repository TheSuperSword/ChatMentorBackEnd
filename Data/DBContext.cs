using ChatMentor.Backend.Model;
using ChatMentor.Backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.DbContext
{
    public partial class ChatMentorDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ChatMentorDbContext(DbContextOptions<ChatMentorDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> TblUser { get; set; }
        public virtual DbSet<Tag> TblTag { get; set; }
        public virtual DbSet<UserTag> TblUserTag { get; set; }
        public virtual DbSet<Document> TblDocument { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define User-Tag Many-to-Many Relationship
            modelBuilder.Entity<UserTag>()
                .HasOne(ut => ut.User)  // One User
                .WithMany(u => u.UserTags)  // Many UserTags
                .HasForeignKey(ut => ut.UserId)  // FK in UserTag table
                .OnDelete(DeleteBehavior.Cascade);  // If User is deleted, delete related UserTags

            modelBuilder.Entity<UserTag>()
                .HasOne(ut => ut.Tag)  // One Tag
                .WithMany(t => t.UserTags)  // Many UserTags
                .HasForeignKey(ut => ut.TagId)  // FK in UserTag table
                .OnDelete(DeleteBehavior.Cascade);  // If Tag is deleted, delete related UserTags
        }

    }
}