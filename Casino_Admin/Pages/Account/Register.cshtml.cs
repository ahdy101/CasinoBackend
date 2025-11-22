using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Casino_Admin.Data;
using Casino_Admin.Models;
using System.Threading.Tasks;

public class RegisterModel : PageModel
{
    private readonly AppDbContext _db;
    public RegisterModel(AppDbContext db) { _db = db; }

    [BindProperty]
    public string Username { get; set; } = string.Empty;
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return Page();
        }
        if (_db.Users.Any(u => u.Username == Username))
        {
            ErrorMessage = "Username already exists.";
            return Page();
        }
        var hash = BCrypt.Net.BCrypt.HashPassword(Password);
        var user = new User { Username = Username, PasswordHash = hash, Balance = 1000m };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return RedirectToPage("/Account/Login");
    }
}