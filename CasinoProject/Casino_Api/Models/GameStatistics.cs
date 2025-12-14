namespace Casino_Api.Models;

public class GameStatistics
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public string GameType { get; set; } = string.Empty;
    
    public int Wins { get; set; }
    
    public int Losses { get; set; }
    
    public int Pushes { get; set; }
    
    public decimal TotalWagered { get; set; }
    
    public decimal TotalWon { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public User User { get; set; } = null!;
}
