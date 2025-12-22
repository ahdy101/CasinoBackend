using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Casino.Backend.Models;

namespace Casino_Api.Models;

public class GameHistory
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string GameType { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BetAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Payout { get; set; }
    
    [MaxLength(50)]
    public string Result { get; set; } = string.Empty;
    
    public string DetailsJson { get; set; } = "{}";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public User User { get; set; } = null!;
}
