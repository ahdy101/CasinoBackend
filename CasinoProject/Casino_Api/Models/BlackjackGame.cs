using System.ComponentModel.DataAnnotations.Schema;

namespace Casino_Api.Models;

public class BlackjackGame
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BetAmount { get; set; }
    
    public string PlayerHandJson { get; set; } = "[]";
    
    public string DealerHandJson { get; set; } = "[]";
    
    public int PlayerTotal { get; set; }
    
    public int DealerTotal { get; set; }
    
    public string Status { get; set; } = "Active";
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Payout { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}

public enum GameStatus
{
    Active,
    PlayerBust,
    DealerBust,
    PlayerBlackjack,
    PlayerWin,
    DealerWin,
    Push
}
