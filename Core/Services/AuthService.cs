using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Services;
using ValidationException = ChatMentor.Backend.Handler.ValidationException;

namespace ChatMentor.Backend.Core.Services;

public class AuthService
{
    private readonly DocumentService _documentService;
    private readonly TokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly UserTagService _userTagService; // Add UserTagService


    public AuthService(IUserRepository userRepository, IWebHostEnvironment hostingEnvironment,
        DocumentService documentService, TokenService tokenService, UserTagService userTagService)
    {
        _userRepository = userRepository;
        _documentService = documentService;
        _tokenService = tokenService;
        _userTagService = userTagService;
    }

    public async Task<UserDto?> RegisterUserAsync(RegisterUserDto dto)
    {
        // Collect validation errors
        var validationErrors = new Dictionary<string, string[]>();

        // Confirm Password
        if (dto.Password != dto.ConfirmPassword) validationErrors.Add("Password", ["Passwords do not match."]);

        // Check if an email is already registered
        if (await _userRepository.IsEmailInUseAsync(dto.Email))
            validationErrors.Add("Email", ["The email address is already registered."]);

        // Validate email format
        if (!new EmailAddressAttribute().IsValid(dto.Email)) validationErrors.Add("Email", ["Invalid email format."]);

        // Throw a ValidationException if there are any errors
        if (validationErrors.Any()) throw new ValidationException(validationErrors);

        // Hash the password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Create user object
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = hashedPassword,
            Headline = dto.Headline,
            Bio = dto.Bio,
            ProfilePictureUrl = "/uploads/profile_pics/default.png",
            Role = dto.Role ?? UserRole.Student // Default to Student if not provided
        };
        // Save user to the database
        var createdUser = await _userRepository.CreateUserAsync(user);

        // Map to DTO
        return new UserDto
        {
            UserGuid = createdUser.UserId.ToString(),
            FirstName = createdUser.FirstName,
            LastName = createdUser.LastName,
            Email = createdUser.Email,
            Headline = createdUser.Headline,
            Bio = createdUser.Bio,
            ProfilePictureUrl = createdUser.ProfilePictureUrl, // Include the profile picture URL
            Role = createdUser.Role,
        };
    }

    public async Task<LoginResponseDto?> LoginUserAsync(LoginUserDto dto)
    {
        var email = dto.Email;
        var password = dto.Password;

        // Retrieve user by email
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null) return null;

        // Check if the account is locked
        if (user.Status == AccountStatus.Suspended)
            throw new UnauthorizedAccessException("Account locked due to too many failed login attempts.");

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            await _userRepository.IncrementPasswordTriesAsync(user);
            if (user.FailedLoginAttempts >= 5)
            {
                await _userRepository.LockUserAsync(user);
                throw new BadHttpRequestException("Account locked due to too many failed login attempts.");
            }

            return null;
        }
        
        // Reset failed login attempts
        await _userRepository.ResetPasswordTriesAsync(user);

        // Update last login time
        await _userRepository.UpdateLastLoginAsync(user);

        // Generate tokens (access token and refresh token)
        var userName = $"{user.FirstName} {user.LastName}";
        var tokenResponse = _tokenService.GenerateTokens(user.UserId.ToString(), userName, user.Role.ToString());

        // Save a refresh token in a database
        await _userRepository.SetRefreshTokenAsync(user, tokenResponse.RefreshToken, tokenResponse.RefreshTokenExpiresAt);
        var userTagsInfo = await _userTagService.GetTagsForUserAsync(user.Id);

        // Create response
        return new LoginResponseDto
        {
            UserGuid = user.UserId.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Headline = user.Headline,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Role = UserRole.Student,
            Tags = userTagsInfo.Tags,
            AccessToken = tokenResponse.AccessToken, // Changed from Token to AccessToken
            RefreshToken = tokenResponse.RefreshToken, // Added RefreshToken
            RefreshTokenExpiresAt = tokenResponse.RefreshTokenExpiresAt // Added ExpiresAt
        };
    }

    public async Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest refreshRequest)
    {
        if (string.IsNullOrEmpty(refreshRequest.AccessToken) ||
            string.IsNullOrEmpty(refreshRequest.RefreshToken)) return null;

        // Extract claims from the expired token
        var principal = _tokenService.GetPrincipalFromExpiredToken(refreshRequest.AccessToken);
        if (principal == null) return null; // Invalid token

        // Extract user ID from claims
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return null; // User ID not found in claims

        // Retrieve user by ID
        var user = await _userRepository.GetUserByGuidAsync(userId);
        if (user == null) return null; // User not found

        // Validate refresh token
        if (user.RefreshToken != refreshRequest.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null; // Invalid or expired refresh token

        // Generate new tokens
        var userName = $"{user.FirstName} {user.LastName}";
        var tokenResponse = _tokenService.GenerateTokens(userId, userName, user.Role.ToString());
        
        // Save new refresh token in the database
        await _userRepository.SetRefreshTokenAsync(user, tokenResponse.RefreshToken, tokenResponse.RefreshTokenExpiresAt);

        return tokenResponse;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var parsedGuid)) return false;

        var user = await _userRepository.GetUserByGuidAsync(userId);
        if (user == null) return false;

        // Clear refresh token
        return await _userRepository.SetRefreshTokenAsync(user, null, DateTime.MinValue);
    }
}