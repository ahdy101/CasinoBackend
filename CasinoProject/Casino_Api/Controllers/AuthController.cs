using Casino_Api.DTOs.Requests;
using Casino_Api.DTOs.Responses;
using Casino_Api.Security;
using Casino_Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Casino_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly TokenValidator _tokenValidator;

    public AuthController(IAuthService authService, TokenValidator tokenValidator)
    {
        _authService = authService;
        _tokenValidator = tokenValidator;
    }

    /// <summary>
    /// Get bearer token using username and password
    /// </summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TokenErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(TokenErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Token(
        [FromQuery] string grant_type,
        [FromQuery] string username,
        [FromQuery] string password,
        [FromQuery] string webapi_key)
    {
        if (string.IsNullOrEmpty(grant_type) || string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(webapi_key))
        {
            return BadRequest(new TokenErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = "Missing required parameters: grant_type, username, password, webapi_key"
            });
        }

        var request = new TokenRequest
        {
            GrantType = grant_type,
            Username = username,
            Password = password,
            WebApiKey = webapi_key
        };

        var result = await _authService.AuthenticateWithToken(request);

        if (!result.Success)
        {
            var statusCode = result.ErrorCode switch
            {
                "invalid_client" => StatusCodes.Status401Unauthorized,
                "unsupported_grant_type" => StatusCodes.Status400BadRequest,
                "invalid_grant" => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };

            return StatusCode(statusCode, new TokenErrorResponse
            {
                Error = result.ErrorCode,
                ErrorDescription = result.Message
            });
        }

        var expiryMinutes = 30;
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(expiryMinutes);

        var response = new TokenResponse
        {
            AccessToken = result.Token,
            TokenType = "Bearer",
            ExpiresIn = expiryMinutes * 60, // Convert to seconds
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            User = result.User!
        };

        return Ok(response);
    }

    /// <summary>
    /// Login with username and password (JSON body)
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new ErrorResponse("Username and password are required"));
        }

        var tokenRequest = new TokenRequest
        {
            GrantType = "password",
            Username = request.Username,
            Password = request.Password,
            WebApiKey = "default_tenant_api_key_12345" // Use default key for login endpoint
        };

        var result = await _authService.AuthenticateWithToken(tokenRequest);

        if (!result.Success)
        {
            return Unauthorized(new ErrorResponse(result.Message));
        }

        var response = new LoginResponse
        {
            Token = result.Token,
            ApiKey = "default_tenant_api_key_12345",
            User = result.User!
        };

        return Ok(response);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new ErrorResponse("Username and password are required"));
        }

        var result = await _authService.Register(request);

        if (!result.Success)
            return BadRequest(new ErrorResponse(result.Message));

        // Return login response with token
        var response = new LoginResponse
        {
            Token = result.Token ?? string.Empty,
            ApiKey = "default_tenant_api_key_12345",
            User = result.User!
        };

        return Ok(response);
    }

    /// <summary>
    /// Get current user information (Requires: Bearer token in Authorization header)
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        // Read directly from Request.Headers
        var authorization = Request.Headers["Authorization"].FirstOrDefault();
        
        // Debug: Log what we're receiving
        Console.WriteLine($"[DEBUG] Authorization from headers: {authorization ?? "NULL"}");
    Console.WriteLine($"[DEBUG] All headers: {string.Join(", ", Request.Headers.Keys)}");

     if (string.IsNullOrEmpty(authorization))
        {
 return Unauthorized(new ErrorResponse("Authorization header is required. Format: Bearer {token}"));
        }

        var validation = _tokenValidator.ValidateToken(authorization);
        if (!validation.IsValid)
            return Unauthorized(new ErrorResponse(validation.Error));

        var result = await _authService.GetUserById(validation.UserId);

        if (!result.Success)
         return NotFound(new ErrorResponse(result.Message));

        return Ok(result.User);
    }
}
