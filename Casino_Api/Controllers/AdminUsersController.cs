using Casino.Backend.Data;
using Casino.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Casino.Backend.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class AdminUsersController : ControllerBase
 {
 private readonly AppDbContext _db;
 public AdminUsersController(AppDbContext db) { _db = db; }

 private bool IsApiKeyValid(string apiKey)
 {
 return !string.IsNullOrEmpty(apiKey) && _db.TenantApiKeys.Any(k => k.ApiKey == apiKey);
 }

 [HttpGet]
 public async Task<IActionResult> GetAll([FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 var admins = await _db.AdminUsers.ToListAsync();
 return Ok(admins);
 }

 [HttpGet("{id}")]
 public async Task<IActionResult> Get(int id, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 var admin = await _db.AdminUsers.FindAsync(id);
 if (admin == null) return NotFound();
 return Ok(admin);
 }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] AdminUser admin, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 // Hash password if not already hashed
 if (!admin.PasswordHash.StartsWith("$2a$") && !admin.PasswordHash.StartsWith("$2b$") && !admin.PasswordHash.StartsWith("$2y$"))
 {
 admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);
 }
 _db.AdminUsers.Add(admin);
 await _db.SaveChangesAsync();
 return CreatedAtAction(nameof(Get), new { id = admin.Id }, admin);
 }

 [HttpPut("{id}")]
 public async Task<IActionResult> Update(int id, [FromBody] AdminUser admin, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 if (id != admin.Id) return BadRequest();
 // Hash password if not already hashed
 if (!admin.PasswordHash.StartsWith("$2a$") && !admin.PasswordHash.StartsWith("$2b$") && !admin.PasswordHash.StartsWith("$2y$"))
 {
 admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);
 }
 _db.Entry(admin).State = EntityState.Modified;
 await _db.SaveChangesAsync();
 return NoContent();
 }

 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 var admin = await _db.AdminUsers.FindAsync(id);
 if (admin == null) return NotFound();
 _db.AdminUsers.Remove(admin);
 await _db.SaveChangesAsync();
 return NoContent();
 }
 }
}
