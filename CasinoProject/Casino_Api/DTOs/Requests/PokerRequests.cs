using System.ComponentModel.DataAnnotations;

namespace Casino_Api.DTOs.Requests;

public class InitializePokerRequest
{
    [Required]
    [Range(1, 100000)]
    public decimal BuyIn { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string GameType { get; set; } = "TexasHoldem";
}

public class PokerActionRequest
{
    [Required]
    public int TableId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // "Check", "Bet", "Fold", "Raise"
    
    public decimal? Amount { get; set; }
}
