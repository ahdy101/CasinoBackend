using Casino_Api.DTOs.Requests;
using Casino_Api.DTOs.Responses;
using Casino_Api.Security;
using Casino_Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(RequireApiKeyAttribute))]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
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
    /// Login with email and password
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
