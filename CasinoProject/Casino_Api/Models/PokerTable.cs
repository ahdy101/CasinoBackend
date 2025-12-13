using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Casino_Api.Models;

public class PokerTable
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string GameType { get; set; } = "TexasHoldem";
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BuyIn { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Pot { get; set; }
    
    [MaxLength(50)]
    public string Stage { get; set; } = "PreFlop";
    
    [MaxLength(50)]
    public string Status { get; set; } = "WaitingForPlayers";
    
    public string CommunityCardsJson { get; set; } = "[]";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
