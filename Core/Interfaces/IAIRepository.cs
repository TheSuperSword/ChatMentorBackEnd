namespace ChatMentor.Backend.Core.Interfaces;

public interface IAIRepository
{
    Task<string> GenerateContentAsync(string prompt);
    Task<string> GenerateCourseOutlineAsync(string courseTitle);
    Task<string> GenerateQuizAsync(string topic);
}
