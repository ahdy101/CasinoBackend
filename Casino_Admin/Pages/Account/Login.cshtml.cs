using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Casino_Admin.Data;
using Casino_Admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;

public class LoginModel : PageModel
{
    private readonly AppDbContext _db;
    public LoginModel(AppDbContext db) { _db = db; }

    [BindProperty]
    public string Username { get; set; } = string.Empty;
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        // Try normal user
        var user = _db.Users.FirstOrDefault(u => u.Username == Username);
        if (user != null && BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToPage("/Index");
        }
        // Try admin user
        var admin = _db.AdminUsers.FirstOrDefault(a => a.Username == Username);
        if (admin != null && BCrypt.Net.BCrypt.Verify(Password, admin.PasswordHash))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToPage("/Index");
        }
        ErrorMessage = "Invalid username or password.";
        return Page();
    }
}