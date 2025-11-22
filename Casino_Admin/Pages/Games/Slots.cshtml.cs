using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Casino_Admin.Data;
using System.Security.Claims;

public class SlotsModel : PageModel
{
 private readonly AppDbContext _db;
 public SlotsModel(AppDbContext db) { _db = db; }
 [BindProperty]
 public int BetAmount { get; set; }
 public string? Result { get; set; }
 public decimal? Balance { get; set; }
 public string? Slots { get; set; }

 private static readonly string[] Symbols = new[] { "??", "??", "??", "7??", "??", "??" };

 public void OnGet()
 {
 LoadBalance();
 }

 public IActionResult OnPost()
 {
 var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
 var user = _db.Users.FirstOrDefault(u => u.Id == userId);
 if (user == null) return RedirectToPage("/Account/Login");
 if (BetAmount <=0 || BetAmount > user.Balance)
 {
 Result = "Invalid bet amount.";
 LoadBalance();
 return Page();
 }
 var rand = new Random();
 int[] idx = { rand.Next(Symbols.Length), rand.Next(Symbols.Length), rand.Next(Symbols.Length) };
 string[] spin = { Symbols[idx[0]], Symbols[idx[1]], Symbols[idx[2]] };
 Slots = string.Join(" ", spin);
 user.Balance -= BetAmount;
 decimal payout =0;
 if (idx[0] == idx[1] && idx[1] == idx[2])
 {
 //3 of a kind
 payout = BetAmount *10;
 Result = $"JACKPOT! You spun {Slots} and won {payout}.";
 }
 else if (idx[0] == idx[1] || idx[1] == idx[2] || idx[0] == idx[2])
 {
 //2 of a kind
 payout = BetAmount *2;
 Result = $"Nice! You spun {Slots} and won {payout}.";
 }
 else if (spin.Contains("7??"))
 {
 // Any7 is a small win
 payout = BetAmount;
 Result = $"Lucky7! You spun {Slots} and got your bet back.";
 }
 else
 {
 Result = $"You spun {Slots} - Sorry, you lost.";
 }
 user.Balance += payout;
 _db.SaveChanges();
 Balance = user.Balance;
 return Page();
 }

 private void LoadBalance()
 {
 var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
 var user = _db.Users.FirstOrDefault(u => u.Id == userId);
 Balance = user?.Balance;
 }
}
