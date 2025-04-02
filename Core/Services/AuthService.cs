using System.ComponentModel.DataAnnotations;
using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model;
using Microsoft.AspNetCore.Identity;
using ValidationException = ChatMentor.Backend.Handler.ValidationException;

namespace ChatMentor.Backend.Core.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly DocumentService _documentService;
    private readonly TokenService _tokenService;

    public AuthService(IUserRepository userRepository, IWebHostEnvironment hostingEnvironment, DocumentService documentService, TokenService tokenService)
    {
        _userRepository = userRepository;
        _documentService = documentService;
        _tokenService = tokenService;
    }

    public async Task<UserDto?> RegisterUserAsync(RegisterUserDto dto, IFormFile? imageFile)
    {
        // Collect validation errors
        var validationErrors = new Dictionary<string, string[]>();

        // Confirm Password
        if (dto.Password != dto.ConfirmPassword) validationErrors.Add("Password", ["Passwords do not match."]);
        
        // Check if email is already registered
        if (await _userRepository.IsEmailInUseAsync(dto.Email)) validationErrors.Add("Email", ["The email address is already registered."]);
        
        // Validate email format
        if (!new EmailAddressAttribute().IsValid(dto.Email)) validationErrors.Add("Email", ["Invalid email format."]);
        
        // Throw a ValidationException if there are any errors
        if (validationErrors.Any()) throw new ValidationException(validationErrors);

        // Hash the password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Create user object
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = hashedPassword,
            Role = dto.Role ?? UserRole.Student // Default to Student if not provided
        };

        // Handle profile picture upload
        if (imageFile != null && imageFile.Length > 0)
        {
            var uploadedDocument = await _documentService.UploadDocumentAsync(
                imageFile,
                Guid.Empty, // Assuming no user ID yet, using Guid.Empty
                "profile_pics"
            );

            if (uploadedDocument != null)
            {
                user.ProfilePictureUrl = uploadedDocument.FilePath; // Use relative URL stored in the document
            }
        }
        else
        {
            // Assign a default profile picture
            user.ProfilePictureUrl = "/uploads/profile_pics/default.png";
        }

        // Save user to the database
        var createdUser = await _userRepository.CreateUserAsync(user);

        // Map to DTO
        return new UserDto
        {
            FirstName = createdUser.FirstName,
            LastName = createdUser.LastName,
            Email = createdUser.Email,
            Headline = createdUser.Headline,
            Bio = createdUser.Bio,
            ProfilePictureUrl = createdUser.ProfilePictureUrl // Include the profile picture URL
        };
    } 
    
    public async Task<LoginResponseDto?> LoginUserAsync(LoginUserDto dto)
    {
        var email = dto.Email;
        var password = dto.Password;
        
        // Retrieve user by email
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null) return null;

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        // Use TokenService to generate JWT
        var token = _tokenService.GenerateToken(user.Id.ToString(), user.FirstName + user.LastName, user.Role.ToString());

        // Create response DTO
        return new LoginResponseDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Headline = user.Headline,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Token = token // Include JWT token in the response
        };
    }
    
}