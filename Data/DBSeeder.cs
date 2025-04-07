using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.Data;

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
                //Qualifications
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
                    
                //Skills
                new Tag { Name = "AI" },
                new Tag { Name = "Software Engineering" },
                new Tag { Name = "Cybersecurity" },
                new Tag { Name = "Data Science" },
                new Tag { Name = "Machine Learning" },
                new Tag { Name = "Cloud Computing" },
                new Tag { Name = "Blockchain" },
                new Tag { Name = "Web Development" },
                new Tag { Name = "Mobile Development" },
                new Tag { Name = "DevOps" },
                new Tag { Name = "Database Administration" },
                new Tag { Name = "Data Engineering" },
                new Tag { Name = "Full Stack Development" },
                new Tag { Name = "UI/UX Design" },
                new Tag { Name = "Product Management" },
                new Tag { Name = "Game Development" },
                new Tag { Name = "Networking" },
                new Tag { Name = "Quantum Computing" },
                new Tag { Name = "Robotics" },
                new Tag { Name = "Artificial Intelligence" },
                new Tag { Name = "Ethical Hacking" },
                new Tag { Name = "Cloud Security" },
                new Tag { Name = "IoT (Internet of Things)" },
                new Tag { Name = "Big Data" },
                new Tag { Name = "Agile Methodology" },
                new Tag { Name = "Software Testing" },
                new Tag { Name = "Game Design" },
                new Tag { Name = "Digital Marketing" },
                new Tag { Name = "SEO (Search Engine Optimization)" },
                new Tag { Name = "Data Analytics" },
                new Tag { Name = "AI Ethics" },
                new Tag { Name = "Business Intelligence" },
                new Tag { Name = "Autonomous Vehicles" }
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