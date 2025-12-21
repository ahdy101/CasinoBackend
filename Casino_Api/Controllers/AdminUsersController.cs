using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
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
 return Ok(admins);
 }

 [HttpGet("{id}")]
 public async Task<IActionResult> Get(int id, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
    return Unauthorized("Invalid or missing API key.");
      
 var admin = await _adminUserRepository.GetByIdAsync(id);
 if (admin == null) return NotFound();
 return Ok(admin);
 }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] AdminUser admin, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
       return Unauthorized("Invalid or missing API key.");
       
 // Hash password if not already hashed
 if (!admin.PasswordHash.StartsWith("$2a$") && !admin.PasswordHash.StartsWith("$2b$") && !admin.PasswordHash.StartsWith("$2y$"))
 {
 admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);
 }
 
 await _adminUserRepository.AddAsync(admin);
 return CreatedAtAction(nameof(Get), new { id = admin.Id, apiKey }, admin);
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
 }
}
