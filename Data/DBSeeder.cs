using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Data;

public static class DbSeeder
{
    public static void Seed(ChatMentorDbContext context)
    {
        // Seed Users if they don't exist
        if (!context.TblUser.Any())
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
                    ProfilePictureUrl = "https://ui-avatars.com/api/?name=Alice+Doe&size=200",
                    Headline = "Computer Science Student",
                    Bio = "Passionate about learning new technologies and programming concepts.",
                    Status = AccountStatus.Active
                },
                new User()
                {
                    FirstName = "Dr. Bob",
                    LastName = "Smith",
                    Email = "bob@example.com",
                    PasswordHash = "hashedpassword456",
                    Role = UserRole.Mentor,
                    ProfilePictureUrl = "https://ui-avatars.com/api/?name=Dr.Bob+Smith&size=200",
                    Headline = "Senior Software Engineer & Mentor",
                    Bio = "Experienced software engineer with 15+ years in the field. Passionate about mentoring new developers.",
                    Status = AccountStatus.Active
                },
                new User()
                {
                    FirstName = "Carol",
                    LastName = "Johnson",
                    Email = "carol@example.com",
                    PasswordHash = "hashedpassword789",
                    Role = UserRole.Student,
                    ProfilePictureUrl = "https://ui-avatars.com/api/?name=Carol+Johnson&size=200",
                    Headline = "Data Science Enthusiast",
                    Bio = "Working on machine learning projects and data analysis techniques.",
                    Status = AccountStatus.Active
                },
                new User()
                {
                    FirstName = "David",
                    LastName = "Wilson",
                    Email = "david@example.com",
                    PasswordHash = "hashedpassword101",
                    Role = UserRole.Mentor,
                    ProfilePictureUrl = "https://ui-avatars.com/api/?name=David+Wilson&size=200",
                    Headline = "Full Stack Developer & AI Researcher",
                    Bio = "Building innovative web applications and exploring AI applications in software development.",
                    Status = AccountStatus.Active
                },
                new User()
                {
                    FirstName = "Eve",
                    LastName = "Martinez",
                    Email = "eve@example.com",
                    PasswordHash = "hashedpassword202",
                    Role = UserRole.Student,
                    ProfilePictureUrl = "https://ui-avatars.com/api/?name=Eve+Martinez&size=200",
                    Headline = "UX Design Student",
                    Bio = "Learning to create intuitive user experiences and accessible interfaces.",
                    Status = AccountStatus.Active
                }
            };

            context.TblUser.AddRange(users);
            context.SaveChanges();
        }

        // Seed Tags if they don't exist
        if (!context.TblTag.Any())
        {
            var tags = new List<Tag>
            {
                new Tag { Name = "First Year Bachelor's" },
                new Tag { Name = "Second Year Bachelor's" },
                new Tag { Name = "Third Year Bachelor's" },
                new Tag { Name = "Final Year Bachelor's" },
                new Tag { Name = "Bachelor's Degree" },
                new Tag { Name = "Diploma" },
                new Tag { Name = "Associate Degree" },
                new Tag { Name = "Master's Degree" },
                new Tag { Name = "PhD" },
                new Tag { Name = "Postdoctoral Research" },
                new Tag { Name = "Graduate" },
                new Tag { Name = "Undergraduate" },
                new Tag { Name = "Foundation Year" },
                new Tag { Name = "Certificate" },
                new Tag { Name = "High School Graduate" },
                new Tag { Name = "Technical Training" },
                new Tag { Name = "Internship" },
                new Tag { Name = "MBA" }, // For business-focused users
                new Tag { Name = "MSc" }, // Master of Science
                new Tag { Name = "MEng" }, // Master of Engineering
                new Tag { Name = "BSc" }, // Bachelor of Science
                new Tag { Name = "BEng" },
                new Tag { Name = "Software Engineering" },
                new Tag { Name = "Web Development" },
                new Tag { Name = "Mobile Development" },
                new Tag { Name = "Data Science" },
                new Tag { Name = "Machine Learning" },
                new Tag { Name = "Artificial Intelligence" },
                new Tag { Name = "Cloud Computing" },
                new Tag { Name = "DevOps" },
                new Tag { Name = "Database Management" },
                new Tag { Name = "UI/UX Design" },
                new Tag { Name = "Frontend Development" },
                new Tag { Name = "Backend Development" },
                new Tag { Name = "Full Stack Development" },
                new Tag { Name = "Python" },
                new Tag { Name = "JavaScript" },
                new Tag { Name = "C#" },
                new Tag { Name = "Java" },
                new Tag { Name = "React" },
                new Tag { Name = "Angular" },
                new Tag { Name = "Node.js" },
                new Tag { Name = "ASP.NET" },
                new Tag { Name = "SQL" },
                new Tag { Name = "NoSQL" },
                new Tag { Name = "Azure" },
                new Tag { Name = "AWS" },
                new Tag { Name = "Docker" },
                new Tag { Name = "Kubernetes" },
                new Tag { Name = "Git" },
                new Tag { Name = "Agile Methodology" },
                new Tag { Name = "Computer Science" }
            };

            context.TblTag.AddRange(tags);
            context.SaveChanges();
        }

        // Seed UserTags if they don't exist
        if (!context.TblUserTag.Any())
        {
            // Get all users and tags from database
            var users = context.TblUser.ToList();
            var tags = context.TblTag.ToList();

            if (!users.Any() || !tags.Any()) return;

            var userTags = new List<UserTag>();

            // Alice's tags (Computer Science Student)
            var alice = users.FirstOrDefault(u => u.Email == "alice@example.com");
            if (alice != null)
            {
                userTags.AddRange(new[]
                {
                    new UserTag { UserId = alice.Id, TagId = tags.First(t => t.Name == "Software Engineering").Id },
                    new UserTag { UserId = alice.Id, TagId = tags.First(t => t.Name == "Python").Id },
                    new UserTag { UserId = alice.Id, TagId = tags.First(t => t.Name == "JavaScript").Id },
                    new UserTag { UserId = alice.Id, TagId = tags.First(t => t.Name == "Computer Science").Id },
                    new UserTag { UserId = alice.Id, TagId = tags.First(t => t.Name == "Web Development").Id }
                });
            }

            // Bob's tags (Senior Software Engineer & Mentor)
            var bob = users.FirstOrDefault(u => u.Email == "bob@example.com");
            if (bob != null)
            {
                userTags.AddRange(new[]
                {
                    new UserTag { UserId = bob.Id, TagId = tags.First(t => t.Name == "Software Engineering").Id },
                    new UserTag { UserId = bob.Id, TagId = tags.First(t => t.Name == "C#").Id },
                    new UserTag { UserId = bob.Id, TagId = tags.First(t => t.Name == "ASP.NET").Id },
                    new UserTag { UserId = bob.Id, TagId = tags.First(t => t.Name == "DevOps").Id },
                    new UserTag { UserId = bob.Id, TagId = tags.First(t => t.Name == "Azure").Id },
                    new UserTag { UserId = bob.Id, TagId = tags.First(t => t.Name == "Database Management").Id },
                    new UserTag { UserId = bob.Id, TagId = tags.First(t => t.Name == "SQL").Id },
                    new UserTag { UserId = bob.Id, TagId = tags.First(t => t.Name == "Agile Methodology").Id }
                });
            }

            // Carol's tags (Data Science Enthusiast)
            var carol = users.FirstOrDefault(u => u.Email == "carol@example.com");
            if (carol != null)
            {
                userTags.AddRange(new[]
                {
                    new UserTag { UserId = carol.Id, TagId = tags.First(t => t.Name == "Data Science").Id },
                    new UserTag { UserId = carol.Id, TagId = tags.First(t => t.Name == "Machine Learning").Id },
                    new UserTag { UserId = carol.Id, TagId = tags.First(t => t.Name == "Python").Id },
                    new UserTag { UserId = carol.Id, TagId = tags.First(t => t.Name == "SQL").Id },
                    new UserTag { UserId = carol.Id, TagId = tags.First(t => t.Name == "Artificial Intelligence").Id }
                });
            }

            // David's tags (Full Stack Developer & AI Researcher)
            var david = users.FirstOrDefault(u => u.Email == "david@example.com");
            if (david != null)
            {
                userTags.AddRange(new[]
                {
                    new UserTag { UserId = david.Id, TagId = tags.First(t => t.Name == "Full Stack Development").Id },
                    new UserTag { UserId = david.Id, TagId = tags.First(t => t.Name == "Frontend Development").Id },
                    new UserTag { UserId = david.Id, TagId = tags.First(t => t.Name == "Backend Development").Id },
                    new UserTag { UserId = david.Id, TagId = tags.First(t => t.Name == "React").Id },
                    new UserTag { UserId = david.Id, TagId = tags.First(t => t.Name == "Node.js").Id },
                    new UserTag { UserId = david.Id, TagId = tags.First(t => t.Name == "JavaScript").Id },
                    new UserTag { UserId = david.Id, TagId = tags.First(t => t.Name == "Artificial Intelligence").Id },
                    new UserTag { UserId = david.Id, TagId = tags.First(t => t.Name == "Machine Learning").Id }
                });
            }

            // Eve's tags (UX Design Student)
            var eve = users.FirstOrDefault(u => u.Email == "eve@example.com");
            if (eve != null)
            {
                userTags.AddRange(new[]
                {
                    new UserTag { UserId = eve.Id, TagId = tags.First(t => t.Name == "UI/UX Design").Id },
                    new UserTag { UserId = eve.Id, TagId = tags.First(t => t.Name == "Frontend Development").Id },
                    new UserTag { UserId = eve.Id, TagId = tags.First(t => t.Name == "JavaScript").Id },
                    new UserTag { UserId = eve.Id, TagId = tags.First(t => t.Name == "React").Id }
                });
            }

            context.TblUserTag.AddRange(userTags);
            context.SaveChanges();
        }
    }
}