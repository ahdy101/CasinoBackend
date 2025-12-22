using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Casino.Backend.DTOs.Requests;
using Casino.Backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Casino.Backend.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class AdminUsersController : ControllerBase
 {
  private readonly IAdminUserRepository _adminUserRepository;
  private readonly ITenantApiKeyRepository _tenantApiKeyRepository;

  public AdminUsersController(
      IAdminUserRepository adminUserRepository,
  ITenantApiKeyRepository tenantApiKeyRepository)
  {
 _adminUserRepository = adminUserRepository;
 _tenantApiKeyRepository = tenantApiKeyRepository;
 }

 private async Task<bool> IsApiKeyValid(string apiKey)
 {
  return await _tenantApiKeyRepository.ValidateApiKeyAsync(apiKey);
 }

 [HttpGet]
 public async Task<IActionResult> GetAll([FromQuery] string apiKey)
 {
   if (!await IsApiKeyValid(apiKey)) 
       return Unauthorized("Invalid or missing API key.");
     
 var admins = await _adminUserRepository.GetAllAsync();
   var response = admins.Select(a => new AdminUserResponse
 {
Id = a.Id,
     Username = a.Username,
    Email = a.Email,
    Role = a.Role,
     CreatedAt = a.CreatedAt
      });
 return Ok(response);
 }

 [HttpGet("{id}")]
 public async Task<IActionResult> Get(int id, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
    return Unauthorized("Invalid or missing API key.");
      
 var admin = await _adminUserRepository.GetByIdAsync(id);
 if (admin == null) return NotFound();
            
   var response = new AdminUserResponse
    {
    Id = admin.Id,
     Username = admin.Username,
       Email = admin.Email,
        Role = admin.Role,
        CreatedAt = admin.CreatedAt
};
 return Ok(response);
 }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] CreateAdminUserRequest request, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
    return Unauthorized("Invalid or missing API key.");

          if (!ModelState.IsValid)
        return BadRequest(ModelState);
       
    // Check if username already exists
   var existingUser = await _adminUserRepository.GetByUsernameAsync(request.Username);
       if (existingUser != null)
    return BadRequest(new { message = "Username already exists" });

        // Create admin user
   var admin = new AdminUser
   {
   Username = request.Username,
    Email = request.Email,
  PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
Role = request.Role,
CreatedAt = DateTime.UtcNow
       };
 
 await _adminUserRepository.AddAsync(admin);
   
var response = new AdminUserResponse
     {
            Id = admin.Id,
     Username = admin.Username,
  Email = admin.Email,
   Role = admin.Role,
          CreatedAt = admin.CreatedAt
  };
   
 return CreatedAtAction(nameof(Get), new { id = admin.Id, apiKey }, response);
 }

 [HttpPut("{id}")]
 public async Task<IActionResult> Update(int id, [FromBody] AdminUser admin, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
       return Unauthorized("Invalid or missing API key.");
  
 if (id != admin.Id) return BadRequest();
 
 // Hash password if not already hashed
 if (!admin.PasswordHash.StartsWith("$2a$") && !admin.PasswordHash.StartsWith("$2b$") && !admin.PasswordHash.StartsWith("$2y$"))
 {
 admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);
 }
 
 await _adminUserRepository.UpdateAsync(admin);
 return NoContent();
 }

 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
      return Unauthorized("Invalid or missing API key.");
      
 var admin = await _adminUserRepository.GetByIdAsync(id);
 if (admin == null) return NotFound();
 
 await _adminUserRepository.DeleteAsync(id);
 return NoContent();
 }

        /// <summary>
        /// Restore a soft-deleted admin user
      /// </summary>
    [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id, [FromQuery] string apiKey)
        {
            if (!await IsApiKeyValid(apiKey))
 return Unauthorized("Invalid or missing API key.");

            var restored = await _adminUserRepository.RestoreAsync(id);
       if (!restored)
     return NotFound(new { message = "Admin user not found or already active" });

    return Ok(new { message = "Admin user restored successfully" });
        }

        /// <summary>
  /// Get all admin users including soft-deleted ones
        /// </summary>
        [HttpGet("all-including-deleted")]
        public async Task<IActionResult> GetAllIncludingDeleted([FromQuery] string apiKey)
        {
            if (!await IsApiKeyValid(apiKey))
     return Unauthorized("Invalid or missing API key.");

   var admins = await _adminUserRepository.GetAllIncludingDeletedAsync();
            var response = admins.Select(a => new AdminUserResponse
            {
          Id = a.Id,
      Username = a.Username,
        Email = a.Email,
   Role = a.Role,
                CreatedAt = a.CreatedAt
        });
 return Ok(response);
        }
 }
}
