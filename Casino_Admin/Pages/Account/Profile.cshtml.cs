using Microsoft.AspNetCore.Mvc.RazorPages;
using Casino_Admin.Data;
using System.Security.Claims;

public class ProfileModel : PageModel
{
 private readonly AppDbContext _db;
 public ProfileModel(AppDbContext db) { _db = db; }
 public string Username { get; set; } = "";
 public decimal Balance { get; set; }
 public DateTime CreatedAt { get; set; }

 public void OnGet()
 {
 var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
 var user = _db.Users.FirstOrDefault(u => u.Id == userId);
 if (user != null)
 {
 Username = user.Username;
 Balance = user.Balance;
 CreatedAt = user.CreatedAt;
 }
 }
}
