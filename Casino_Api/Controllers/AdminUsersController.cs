using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Casino.Backend.DTOs.Requests;
using Casino.Backend.DTOs.Responses;
using Casino.Backend.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Casino.Backend.Controllers
{
 [Authorize(Roles = "Admin")]
 [ApiController]
 [Route("api/[controller]")]
 public class AdminUsersController : ControllerBase
 {
  private readonly IUserRepository _userRepository;
  private readonly ILogger<AdminUsersController> _logger;

  public AdminUsersController(
      IUserRepository userRepository,
      ILogger<AdminUsersController> logger)
  {
 _userRepository = userRepository;
 _logger = logger;
 }

 [HttpGet]
 public async Task<IActionResult> GetAll()
 {
 var users = await _userRepository.GetUsersByRoleAsync("Admin");
   var response = users.Select(u => new UserResponse
 {
Id = u.Id,
   Username = u.Username,
    Email = u.Email,
    Balance = u.Balance,
  CreatedAt = u.CreatedAt
      });
 return Ok(response);
 }

 [HttpGet("{id}")]
 public async Task<IActionResult> Get(int id)
 {
 var user = await _userRepository.GetByIdAsync(id);
 if (user == null) return NotFound();

   var etag = ETagHelper.GenerateETag(user.Id, user.ModifiedAt);
   Response.Headers.Append("ETag", $"\"{etag}\"");
 
   var response = new UserResponse
    {
    Id = user.Id,
     Username = user.Username,
       Email = user.Email,
      Balance = user.Balance,
    CreatedAt = user.CreatedAt
};
 return Ok(response);
 }

 [HttpPut("{id}")]
 public async Task<IActionResult> Update(int id, [FromBody] UpdateProfileRequest request, [FromHeader(Name = "If-Match")] string ifMatch)
 {
 if (string.IsNullOrWhiteSpace(ifMatch))
      return BadRequest(new ErrorResponse { Message = "If-Match header required" });

   var current = await _userRepository.GetByIdAsync(id);
   if (current == null) return NotFound();

   var currentETag = ETagHelper.GenerateETag(current.Id, current.ModifiedAt);
   if (!ETagHelper.ValidateETag(ifMatch, currentETag))
   return StatusCode(412, new ErrorResponse { Message = "Resource modified by another user" });
 
   if (!string.IsNullOrWhiteSpace(request.Username))
 current.Username = request.Username;
   if (!string.IsNullOrWhiteSpace(request.Email))
      current.Email = request.Email;
   
 current.ModifiedAt = DateTime.UtcNow;
 await _userRepository.UpdateAsync(current);

   var newETag = ETagHelper.GenerateETag(current.Id, current.ModifiedAt);
   Response.Headers.Append("ETag", $"\"{newETag}\"");

 return NoContent();
 }

 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id, [FromHeader(Name = "If-Match")] string ifMatch)
 {
   if (string.IsNullOrWhiteSpace(ifMatch))
      return BadRequest(new ErrorResponse { Message = "If-Match header required" });
      
 var user = await _userRepository.GetByIdAsync(id);
 if (user == null) return NotFound();

   var currentETag = ETagHelper.GenerateETag(user.Id, user.ModifiedAt);
   if (!ETagHelper.ValidateETag(ifMatch, currentETag))
      return StatusCode(412, new ErrorResponse { Message = "Resource modified by another user" });
 
 await _userRepository.DeleteAsync(id);
 return NoContent();
 }
 }
}
