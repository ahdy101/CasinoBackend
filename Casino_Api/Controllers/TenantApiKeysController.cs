using Casino.Backend.Data;
using Casino.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Casino.Backend.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class TenantApiKeysController : ControllerBase
 {
 private readonly AppDbContext _db;
 public TenantApiKeysController(AppDbContext db) { _db = db; }

 private bool IsApiKeyValid(string apiKey)
 {
 return !string.IsNullOrEmpty(apiKey) && _db.TenantApiKeys.Any(k => k.ApiKey == apiKey);
 }

 [HttpGet]
 public async Task<IActionResult> GetAll([FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 var keys = await _db.TenantApiKeys.ToListAsync();
 return Ok(keys);
 }

 [HttpGet("{id}")]
 public async Task<IActionResult> Get(int id, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 var key = await _db.TenantApiKeys.FindAsync(id);
 if (key == null) return NotFound();
 return Ok(key);
 }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] TenantApiKey key, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 _db.TenantApiKeys.Add(key);
 await _db.SaveChangesAsync();
 return CreatedAtAction(nameof(Get), new { id = key.Id }, key);
 }

 [HttpPut("{id}")]
 public async Task<IActionResult> Update(int id, [FromBody] TenantApiKey key, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 if (id != key.Id) return BadRequest();
 _db.Entry(key).State = EntityState.Modified;
 await _db.SaveChangesAsync();
 return NoContent();
 }

 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id, [FromQuery] string apiKey)
 {
 if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
 var key = await _db.TenantApiKeys.FindAsync(id);
 if (key == null) return NotFound();
 _db.TenantApiKeys.Remove(key);
 await _db.SaveChangesAsync();
 return NoContent();
 }
 }
}
