using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

public class GameDetailsModel : PageModel
{
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;

    public void OnGet(int id)
    {
   GameId = id;
        GameName = id switch
        {
      1 => "Blackjack",
  2 => "Roulette",
        3 => "Slots",
        _ => "Unknown Game"
    };
    }
}