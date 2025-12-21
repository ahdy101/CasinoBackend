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
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromQuery] string username,
        [FromQuery] string email,
        [FromQuery] string password,
        [FromQuery] string apiKey)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(apiKey))
        {
            return BadRequest(new ErrorResponse("Missing required parameters"));
        }

        // Validate API key
        var unitOfWork = HttpContext.RequestServices.GetRequiredService<Repositories.Interfaces.IUnitOfWork>();
        var isValidApiKey = await unitOfWork.TenantApiKeys.ValidateApiKeyAsync(apiKey);
        if (!isValidApiKey)
        {
            return Unauthorized(new ErrorResponse("Invalid API key"));
        }

        var request = new RegisterRequest
        {
            Username = username,
            Email = email,
            Password = password
        };

        var result = await _authService.Register(request);

        if (!result.Success)
            return BadRequest(new ErrorResponse(result.Message));

        return Ok(result.User);
    }

    /// <summary>
    /// Get current user information (Requires: Bearer token in Authorization header)
    /// </summary>
    /// <param name="authorization">Format: Bearer {your_access_token}</param>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(
        [FromHeader(Name = "Authorization")] string? authorization)
    {
        // Try to get from parameter first, then fallback to Request.Headers
        if (string.IsNullOrEmpty(authorization))
        {
            authorization = Request.Headers["Authorization"].FirstOrDefault();
        }

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
