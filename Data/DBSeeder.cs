using ChatMentor.Backend.DbContext;
using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Data
{
    public static class DbSeeder
    {
        public static void Seed(ChatMentorDbContext context)
        {
            if (!context.TblUser.Any()) // Check if users exist
            {
                var users = new List<User>
                {
                    new User()
                    {
                        FirstName = "Alice",
                        LastName = "Doe",
                        Email = "alice@example.com",
                        PasswordHash = "hashedpassword123",
                        Role = UserRole.Student,
                        ProfilePictureUrl = "https://ui-avatars.com/api/?name=Alice+Doe&size=200\n",
                        Status = AccountStatus.Active
                    },
                    new User()
                    {
                        FirstName = "Dr. Bob",
                        LastName = "Smith",
                        Email = "bob@example.com",
                        PasswordHash = "hashedpassword456",
                        Role = UserRole.Mentor,
                        ProfilePictureUrl = "https://ui-avatars.com/api/?name=Dr.Bob+Smith&size=200\n",
                        Status = AccountStatus.Active
                    }
                };

                context.TblUser.AddRange(users);
                context.SaveChanges();
            }

            if (!context.TblTag.Any()) // Check if tags exist
            {
                var tags = new List<Tag>
                {
                    new Tag { Name = "AI" },
                    new Tag { Name = "Software Engineering" },
                    new Tag { Name = "Cybersecurity" }
                };

                context.TblTag.AddRange(tags);
                context.SaveChanges();
            }

            if (context.TblUserTag.Any()) return; // Check if user-tags exist
            var user = context.TblUser.FirstOrDefault(u => u.Email == "alice@example.com");
            var tag = context.TblTag.FirstOrDefault(t => t.Name == "Software Engineering");

            if (user == null || tag == null) return;
            context.TblUserTag.Add(new UserTag { UserId = user.Id, TagId = tag.Id });
            context.SaveChanges();
        }
    }
}
