using Casino.Backend.DTOs.Requests;
using Casino.Backend.DTOs.Responses;
using Casino.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Casino.Backend.Controllers
{
    /// <summary>
    /// Authentication controller for user registration and login
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
      private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
       _authService = authService;
         _logger = logger;
        }

        /// <summary>
    /// Register a new user - No API key required
      /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
    if (!ModelState.IsValid)
     {
        return BadRequest(new ErrorResponse
  {
           Message = "Validation failed",
    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
     });
   }

      try
 {
     var user = await _authService.RegisterAsync(
    request.Username,
   request.Password,
  request.InitialBalance ?? 1000m);

     var response = new UserResponse
   {
      Id = user.Id,
            Username = user.Username,
          Balance = user.Balance,
          CreatedAt = user.CreatedAt
    };

    _logger.LogInformation("User registered successfully - UserId: {UserId}, Username: {Username}",
    user.Id, user.Username);

          return CreatedAtAction(nameof(Register), new { id = user.Id }, response);
            }
 catch (Exception ex)
    {
          _logger.LogWarning(ex, "Registration failed for username: {Username}", request.Username);
                return Conflict(new ErrorResponse
 {
            Message = ex.Message,
         Errors = new List<string> { ex.Message }
                });
            }
        }

   /// <summary>
        /// Login and receive JWT token - No API key required
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
       {
           return BadRequest(new ErrorResponse
        {
         Message = "Validation failed",
    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
    });
        }

  try
      {
         var token = await _authService.LoginAsync(request.Username, request.Password);

     _logger.LogInformation("User logged in successfully - Username: {Username}", request.Username);

                return Ok(new LoginResponse
                {
  Token = token,
    Username = request.Username
    });
  }
         catch (Exception ex)
    {
         _logger.LogWarning(ex, "Login failed for username: {Username}", request.Username);
         return Unauthorized(new ErrorResponse
 {
    Message = "Invalid credentials",
          Errors = new List<string> { "Username or password is incorrect" }
        });
       }
    }
    }

    /// <summary>
    /// Login request model
  /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Username
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
 public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password
        /// </summary>
   [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = string.Empty;
 }

    /// <summary>
    /// Login response with JWT token
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// JWT token for authentication
        /// </summary>
        public string Token { get; set; } = string.Empty;

  /// <summary>
        /// Username
     /// </summary>
        public string Username { get; set; } = string.Empty;
    }
}
