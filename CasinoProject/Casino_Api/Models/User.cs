using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Casino_Api.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0m;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? TenantId { get; set; }
    
    // Navigation properties
    public ICollection<Bet> Bets { get; set; } = new List<Bet>();
    public ICollection<BlackjackGame> BlackjackGames { get; set; } = new List<BlackjackGame>();
    public ICollection<GameHistory> GameHistories { get; set; } = new List<GameHistory>();
}
