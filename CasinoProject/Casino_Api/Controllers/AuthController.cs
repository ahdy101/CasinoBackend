using Casino_Api.DTOs.Requests;
using Casino_Api.DTOs.Responses;
using Casino_Api.Security;
using Casino_Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// OAuth2-style token endpoint - Get bearer token using grant_type=password
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/token
    ///     Content-Type: application/x-www-form-urlencoded
    ///     
    ///     grant_type=password&amp;username=user@example.com&amp;password=yourpassword&amp;webapi_key=default_tenant_api_key_12345
    ///     
    /// Or as JSON:
  /// 
    ///     POST /api/auth/token
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "grant_type": "password",
    ///  "username": "user@example.com",
    ///       "password": "yourpassword",
    ///       "webapi_key": "default_tenant_api_key_12345"
///     }
    /// </remarks>
    [HttpPost("token")]
    [Consumes("application/json", "application/x-www-form-urlencoded")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TokenErrorResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(TokenErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        // Also support JSON body
        if (string.IsNullOrEmpty(request.GrantType) && Request.HasJsonContentType())
        {
      request = await Request.ReadFromJsonAsync<TokenRequest>() ?? request;
        }

        if (!ModelState.IsValid)
     {
            return BadRequest(new TokenErrorResponse
      {
      Error = "invalid_request",
          ErrorDescription = "Missing or invalid parameters"
   });
        }

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
    [ServiceFilter(typeof(RequireApiKeyAttribute))]
  [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        return BadRequest(new ErrorResponse(ModelState));

        var result = await _authService.Register(request);
      
        if (!result.Success)
    return BadRequest(new ErrorResponse(result.Message));

        return Ok(result.User);
    }

    /// <summary>
    /// Login with email and password (Legacy endpoint)
    /// </summary>
    [HttpPost("login")]
  [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse(ModelState));

        var result = await _authService.Login(request);
        
 if (!result.Success)
          return Unauthorized(new ErrorResponse(result.Message));

      var response = new LoginResponse
     {
         Token = result.Token,
         ApiKey = result.ApiKey,
            User = result.User!
        };

        return Ok(response);
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
  var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _authService.GetUserById(userId);

        if (!result.Success)
        return NotFound(new ErrorResponse(result.Message));

        return Ok(result.User);
    }
}
