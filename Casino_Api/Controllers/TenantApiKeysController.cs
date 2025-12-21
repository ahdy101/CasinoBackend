using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Casino.Backend.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 public class TenantApiKeysController : ControllerBase
 {
  private readonly ITenantApiKeyRepository _tenantApiKeyRepository;

  public TenantApiKeysController(ITenantApiKeyRepository tenantApiKeyRepository)
  {
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
      
 var keys = await _tenantApiKeyRepository.GetAllAsync();
 return Ok(keys);
 }

 [HttpGet("{id}")]
 public async Task<IActionResult> Get(int id, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
    return Unauthorized("Invalid or missing API key.");
   
 var key = await _tenantApiKeyRepository.GetByIdAsync(id);
 if (key == null) return NotFound();
 return Ok(key);
 }

 [HttpPost]
 public async Task<IActionResult> Create([FromBody] TenantApiKey key, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
      return Unauthorized("Invalid or missing API key.");
      
 await _tenantApiKeyRepository.AddAsync(key);
 return CreatedAtAction(nameof(Get), new { id = key.Id, apiKey }, key);
 }

 [HttpPut("{id}")]
 public async Task<IActionResult> Update(int id, [FromBody] TenantApiKey key, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
      return Unauthorized("Invalid or missing API key.");
  
 if (id != key.Id) return BadRequest();
 
 await _tenantApiKeyRepository.UpdateAsync(key);
 return NoContent();
 }

 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id, [FromQuery] string apiKey)
 {
  if (!await IsApiKeyValid(apiKey)) 
      return Unauthorized("Invalid or missing API key.");
      
 var key = await _tenantApiKeyRepository.GetByIdAsync(id);
 if (key == null) return NotFound();
 
 await _tenantApiKeyRepository.DeleteAsync(id);
 return NoContent();
 }
 }
}
