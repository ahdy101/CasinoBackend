using Casino_Api.Models;

namespace Casino_Api.DTOs.Responses;

public class BlackjackGameResponse
{
    public int GameId { get; set; }
    public List<Card> PlayerHand { get; set; } = new();
    public List<Card> DealerHand { get; set; } = new();
    public int PlayerTotal { get; set; }
    public int DealerTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal BetAmount { get; set; }
    public decimal? Payout { get; set; }
    public bool CanHit { get; set; }
    public bool CanStand { get; set; }
    public bool CanDoubleDown { get; set; }
    public bool CanSplit { get; set; }
}
