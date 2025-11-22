using Casino.Backend.Data;
using Casino.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Casino.Backend.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class UsersController : ControllerBase
 {
 private readonly AppDbContext _db;
 public UsersController(AppDbContext db) { _db = db; }

 private bool IsApiKeyValid(string apiKey)
 {
 return !string.IsNullOrEmpty(apiKey) && _db.TenantApiKeys.Any(k => k.ApiKey == apiKey);
 }

 [HttpGet]
 public async Task<IActionResult> GetAll([FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 var users = await _db.Users.ToListAsync();
 return Ok(users);
 }

 [HttpGet("{id}")]
 public async Task<IActionResult> Get(int id, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 var user = await _db.Users.FindAsync(id);
 if (user == null) return NotFound();
 return Ok(user);
 }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] User user, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 _db.Users.Add(user);
 await _db.SaveChangesAsync();
 return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
 }

 [HttpPut("{id}")]
 public async Task<IActionResult> Update(int id, [FromBody] User user, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 if (id != user.Id) return BadRequest();
 _db.Entry(user).State = EntityState.Modified;
 await _db.SaveChangesAsync();
 return NoContent();
 }

 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 var user = await _db.Users.FindAsync(id);
 if (user == null) return NotFound();
 _db.Users.Remove(user);
 await _db.SaveChangesAsync();
 return NoContent();
 }
 }
}
