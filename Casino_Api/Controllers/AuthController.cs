using Casino.Backend.DTOs.Requests;
using Casino.Backend.DTOs.Responses;
using Casino.Backend.Services.Interfaces;
using Casino.Backend.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace Casino.Backend.Controllers
{
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
       request.Email,
   request.Password);

      // Generate JWT token for automatic login
            var token = await _authService.LoginAsync(request.Username, request.Password);

var response = new UserResponse
   {
      Id = user.Id,
    Username = user.Username,
   Email = user.Email,
Balance = user.Balance,
        CreatedAt = user.CreatedAt
 };

    _logger.LogInformation("User registered successfully - UserId: {UserId}, Username: {Username}, Email: {Email}",
    user.Id, user.Username, user.Email);

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new
      {
     token = token,
       user = response
       });
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

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        [HttpGet("whoami")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
     public async Task<IActionResult> WhoAmI(
            [FromHeader(Name = "Authorization")][System.ComponentModel.DataAnnotations.Required] string authorization)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
   
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
       {
           return Unauthorized(new ErrorResponse
      {
         Message = "Invalid or missing authentication token",
              Errors = new List<string> { "Please login to access this endpoint" }
   });
         }

        var user = await _authService.GetUserByIdAsync(userId);
    
      if (user == null)
   {
  return NotFound(new ErrorResponse
      {
  Message = "User not found",
       Errors = new List<string> { "User account may have been deleted" }
    });
   }

   // Log ModifiedAt value for debugging
   _logger.LogInformation("WhoAmI - UserId: {UserId}, ModifiedAt: {ModifiedAt}", user.Id, user.ModifiedAt);

   // Generate and add ETag to response headers
   var etag = ETagHelper.GenerateETag(user.Id, user.ModifiedAt);
   _logger.LogInformation("WhoAmI - Generated ETag: {ETag}", etag);
 Response.Headers.Append("ETag", $"\"{etag}\"");

         var response = new UserResponse
    {
  Id = user.Id,
     Username = user.Username,
  Email = user.Email,
     Balance = user.Balance,
        CreatedAt = user.CreatedAt
       };

  _logger.LogInformation("WhoAmI called - UserId: {UserId}", userId);

  return Ok(response);
    }

        /// <summary>
/// Update user profile (username, email) - Requires JWT
    /// </summary>
        [Authorize]
  [HttpPut("profile")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
 [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status412PreconditionFailed)]
  [SwaggerOperation(Summary = "Update user profile", Description = "Requires ETag in If-Match header from GET /api/auth/whoami")]
        public async Task<IActionResult> UpdateProfile(
            [FromHeader(Name = "Authorization")][System.ComponentModel.DataAnnotations.Required] string authorization,
    [FromBody] UpdateProfileRequest request,
       [FromHeader(Name = "If-Match")] string ifMatch)
  {
   var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
   if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
          return Unauthorized(new ErrorResponse { Message = "Invalid token" });

   if (string.IsNullOrWhiteSpace(ifMatch))
      return BadRequest(new ErrorResponse { Message = "If-Match header required" });

      try
     {
     var currentUser = await _authService.GetUserByIdAsync(userId);
    if (currentUser == null)
     return NotFound(new ErrorResponse { Message = "User not found" });

  var currentETag = ETagHelper.GenerateETag(currentUser.Id, currentUser.ModifiedAt);
       
       _logger.LogInformation("ETag Validation - Received: '{ReceivedETag}', Expected: '{ExpectedETag}', ModifiedAt: {ModifiedAt}", 
           ifMatch, currentETag, currentUser.ModifiedAt);
       
       if (!ETagHelper.ValidateETag(ifMatch, currentETag))
      return StatusCode(412, new ErrorResponse { Message = $"Resource modified by another user. Received ETag: {ifMatch}, Expected: {currentETag}" });

      var user = await _authService.UpdateProfileAsync(userId, request.Username, request.Email);
    
  var newETag = ETagHelper.GenerateETag(user.Id, user.ModifiedAt);
    Response.Headers.Append("ETag", $"\"{newETag}\"");

  return Ok(new UserResponse { Id = user.Id, Username = user.Username, Email = user.Email, Balance = user.Balance, CreatedAt = user.CreatedAt });
       }
  catch (Exception ex)
         {
       _logger.LogError(ex, "Error updating profile");
       return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
  /// Change password - Requires JWT and current password
     /// </summary>
        [Authorize]
 [HttpPost("change-password")]
 [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
     [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> ChangePassword(
  [FromHeader(Name = "Authorization")][System.ComponentModel.DataAnnotations.Required] string authorization,
 [FromBody] ChangePasswordRequest request)
    {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
    return Unauthorized(new ErrorResponse { Message = "Invalid token" });

   try
{
  var success = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                if (!success)
     return BadRequest(new ErrorResponse { Message = "Current password is incorrect" });

  _logger.LogInformation("Password changed - UserId: {UserId}", userId);
return Ok(new { message = "Password changed successfully" });
     }
            catch (Exception ex)
   {
_logger.LogError(ex, "Error changing password");
        return BadRequest(new ErrorResponse { Message = ex.Message });
      }
        }

   /// <summary>
        /// Forgot password - Sends reset email (public endpoint)
     /// </summary>
  [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
  try
 {
    var message = await _authService.ForgotPasswordAsync(request.Email);
    return Ok(new { message });
  }
catch (Exception ex)
 {
        _logger.LogError(ex, "Error in forgot password");
 return Ok(new { message = "If the email exists, a reset link has been sent" });
    }
  }

   /// <summary>
 /// Reset password using token from email (public endpoint)
     /// </summary>
        [HttpPost("reset-password")]
 [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
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
   var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
      
        if (!success)
       {
    return BadRequest(new ErrorResponse
        {
      Message = "Invalid or expired reset token",
    Errors = new List<string> { "The password reset link is invalid or has expired. Please request a new one." }
    });
   }

   _logger.LogInformation("Password reset successful");
       return Ok(new { message = "Password reset successfully. You can now login with your new password." });
}
    catch (Exception ex)
  {
_logger.LogError(ex, "Error resetting password");
      return BadRequest(new ErrorResponse 
 { 
    Message = "An error occurred while resetting password",
  Errors = new List<string> { ex.Message }
   });
  }
      }
    }

    public class LoginRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
      public string Username { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
     public string Username { get; set; } = string.Empty;
 }
}
