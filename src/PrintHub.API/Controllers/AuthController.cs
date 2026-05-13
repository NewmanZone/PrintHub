using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace PrintHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly Action<ILogger, string, Exception?> LogLoginAttempt =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(LogLoginAttempt)),
            "Login attempt for {Email}");

    private static readonly Action<ILogger, string, Exception?> LogRegistrationAttempt =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(LogRegistrationAttempt)),
            "Registration attempt for {Email}");

    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(
        ILogger<AuthController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Login endpoint - validates credentials and returns JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email and password are required" });
        }

        // TODO: Validate against user database
        // This is a placeholder for actual authentication logic
        LogLoginAttempt(_logger, request.Email, null);

        // For development, accept any non-empty credentials
        // In production, validate against the user store
        var userId = "user-" + request.Email.GetHashCode();

        var token = GenerateJwtToken(userId, request.Email);

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UserId = userId
        });
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email and password are required" });
        }

        // TODO: Create user in database
        LogRegistrationAttempt(_logger, request.Email, null);

        var userId = "user-" + Guid.NewGuid().ToString("N");

        return Created($"/api/users/{userId}", new RegisterResponse
        {
            UserId = userId,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        });
    }

    private string GenerateJwtToken(string userId, string email)
    {
        var secret = _configuration["Auth:Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT secret not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Auth:Jwt:Issuer"] ?? "PrintHub",
            audience: _configuration["Auth:Jwt:Audience"] ?? "PrintHubApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public class RegisterResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
