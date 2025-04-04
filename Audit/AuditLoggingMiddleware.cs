using System.Security.Claims;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model;
// Replace with your actual namespace for models

namespace ChatMentor.Backend.Handler;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }
        
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;
            
        // Capture the user id after the request is processed
        var userId = GetCurrentUserId(context);

        // Read the request body for logging purposes (if needed)
        request.EnableBuffering();
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Seek(0, SeekOrigin.Begin); // Reset stream position for further use

        // Create a log entry
        var logEntry = new AuditLog
        {
            UserId = userId,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            RequestBody = requestBody,
            StatusCode = response.StatusCode,  // This will capture the final status code
            CreatedAt = DateTime.UtcNow
        };

        // Create a scope to resolve scoped services within the request
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ChatMentorDbContext>();

            // Add a response body capture so we can access the status code after the request has been completed
            var originalBodyStream = response.Body;
            using (var memoryStream = new MemoryStream())
            {
                response.Body = memoryStream;

                // Continue processing the request
                await _next(context);

                // After the request completes, capture the final status code
                logEntry.StatusCode = response.StatusCode;

                // Log the final audit entry to the database
                await dbContext.TblAuditLogs.AddAsync(logEntry);
                await dbContext.SaveChangesAsync();

                // Copy the content of the memory stream to the original response body
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
    }
        
    private Guid? GetCurrentUserId(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userId, out var guid) ? guid : new Guid("00000000-0000-0000-0000-000000000000");
    }
}