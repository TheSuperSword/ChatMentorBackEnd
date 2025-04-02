namespace ChatMentor.Backend.Handler;

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }
}