using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChatMentor.Backend.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace ChatMentor.Backend.Core.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;

        var secretKey = _configuration["Jwt:Secret"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException("JWT Secret key is missing in configuration.");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public string GenerateToken(string userId, string username, string userRole)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId), // UserId or Unique Identifier
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique Token ID
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),

            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, userRole)
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        if (!int.TryParse(_configuration["Jwt:AccessTokenExpiresInMinutes"],
                out var expiresInMinutes)) expiresInMinutes = 15; // Default to 60 minutes if parsing fails

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Generate both access and refresh tokens
    public TokenResponse GenerateTokens(string userId, string username, string userRole)
    {
        var accessToken = GenerateToken(userId, username, userRole);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiresInDays"] ?? "7")); // Default to 7 days
        
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
        };
    }

    // Generate refresh token
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64]; // Using 64 bytes for better security
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    // Validate JWT token and extract claims
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // We'll validate these separately if needed
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateLifetime = false // Don't validate lifetime for expired tokens
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            // Additional security check - make sure it's a JWT token with the right algorithm
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            // If token validation fails for any reason, return null
            return null;
        }
    }
}