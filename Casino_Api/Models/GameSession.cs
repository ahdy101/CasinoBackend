namespace Casino.Backend.Models
{
    /// <summary>
    /// Game session tracking - links Unity game sessions to users
    /// </summary>
  public class GameSession
    {
public int Id { get; set; }
 public int UserId { get; set; }
    public string GameType { get; set; } = string.Empty; // "slots", "roulette", "blackjack", "poker"
        public string GameId { get; set; } = string.Empty;   // Specific game identifier
        public string SessionToken { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";       // Active, Completed, Expired
        public decimal TotalBet { get; set; } = 0;
        public decimal TotalWin { get; set; } = 0;
        public int RoundsPlayed { get; set; } = 0;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
     
   // Navigation
        public User? User { get; set; }
  }
}
