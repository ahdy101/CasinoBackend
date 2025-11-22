using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

public class LogoutModel : PageModel
{
 public async Task<IActionResult> OnGet()
 {
 await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
 return RedirectToPage("/Index");
 }
}
