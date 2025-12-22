using Casino.Backend.DTOs.Requests;
using Casino.Backend.DTOs.Responses;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Casino.Backend.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class UsersController : ControllerBase
 {
private readonly IUserRepository _userRepository;
  private readonly ITenantApiKeyRepository _tenantApiKeyRepository;
 private readonly ILogger<UsersController> _logger;

 public UsersController(
      IUserRepository userRepository,
     ITenantApiKeyRepository tenantApiKeyRepository,
     ILogger<UsersController> logger)
 {
 _userRepository = userRepository;
  _tenantApiKeyRepository = tenantApiKeyRepository;
 _logger = logger;
 }

 private async Task<bool> IsApiKeyValid(string apiKey)
 {
  return await _tenantApiKeyRepository.ValidateApiKeyAsync(apiKey);
 }

 [HttpGet]
 [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
 [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
 public async Task<IActionResult> GetAll([FromQuery] string apiKey)
 {
   if (!await IsApiKeyValid(apiKey))
 return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

 var users = await _userRepository.GetAllAsync();
   var response = users.Select(u => new UserResponse
 {
 Id = u.Id,
 Username = u.Username,
 Balance = u.Balance,
 CreatedAt = u.CreatedAt
 }).ToList();

 return Ok(response);
 }

 [HttpGet("{id}")]
 [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
 [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
 public async Task<IActionResult> Get(int id, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey))
 return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

 var user = await _userRepository.GetByIdAsync(id);
 if (user == null)
 return NotFound(new ErrorResponse { Message = "User not found" });

 var response = new UserResponse
 {
 Id = user.Id,
 Username = user.Username,
 Balance = user.Balance,
 CreatedAt = user.CreatedAt
 };

 return Ok(response);
 }

 [HttpPost]
 [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
 [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
 public async Task<IActionResult> Create([FromBody] CreateUserRequest request, [FromQuery] string apiKey)
 {
 if (!await IsApiKeyValid(apiKey))
 return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

 if (!ModelState.IsValid)
 {
 return BadRequest(new ErrorResponse
 {
 Message = "Validation failed",
 Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
 });
 }

 // Check if username already exists
 if (await _userRepository.UsernameExistsAsync(request.Username))
 {
 return Conflict(new ErrorResponse { Message = "Username already taken" });
 }

 // Hash password
 var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

 var user = new User
 {
 Username = request.Username,
   Email = request.Email,
 PasswordHash = passwordHash,
 Balance = 10000m,
            Role = "Player",
 CreatedAt = DateTime.UtcNow
 };

 await _userRepository.AddAsync(user);

 var response = new UserResponse
 {
 Id = user.Id,
 Username = user.Username,
 Balance = user.Balance,
 CreatedAt = user.CreatedAt
 };

 return CreatedAtAction(nameof(Get), new { id = user.Id, apiKey }, response);
 }

 [HttpPut("{id}")]
 [ProducesResponseType(StatusCodes.Status204NoContent)]
 [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
 [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
 public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, [FromQuery] string apiKey)
 {
 if (!await IsApiKeyValid(apiKey))
 return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

 if (id != request.Id)
 return BadRequest(new ErrorResponse { Message = "ID mismatch" });

 var user = await _userRepository.GetByIdAsync(id);
 if (user == null)
 return NotFound(new ErrorResponse { Message = "User not found" });

 // Update username if provided
 if (!string.IsNullOrEmpty(request.Username))
 {
 user.Username = request.Username;
 }

 // Update password if provided
 if (!string.IsNullOrEmpty(request.NewPassword))
 {
 user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
 }

 await _userRepository.UpdateAsync(user);
 return NoContent();
 }

 [HttpDelete("{id}")]
 [ProducesResponseType(StatusCodes.Status204NoContent)]
 [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
 public async Task<IActionResult> Delete(int id, [FromQuery] string apiKey)
 {
 if (!await IsApiKeyValid(apiKey))
 return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

 var user = await _userRepository.GetByIdAsync(id);
 if (user == null)
 return NotFound(new ErrorResponse { Message = "User not found" });

 await _userRepository.DeleteAsync(id);
 return NoContent();
 }
 }
}
