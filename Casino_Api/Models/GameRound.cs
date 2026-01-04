namespace Casino.Backend.Models
{
    /// <summary>
    /// Individual game round/spin - records each play from Unity
    /// </summary>
    public class GameRound
    {
    public int Id { get; set; }
 public int SessionId { get; set; }
   public int UserId { get; set; }
   public string GameType { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
      public decimal BetAmount { get; set; }
     public decimal WinAmount { get; set; }
     public decimal Multiplier { get; set; } = 0;
        
        /// <summary>
     /// JSON data with game-specific result details
    /// e.g., slot reels, roulette number, cards dealt
        /// </summary>
        public string ResultData { get; set; } = string.Empty;
        
    /// <summary>
        /// Hash for provably fair verification
        /// </summary>
     public string? ServerSeed { get; set; }
   public string? ClientSeed { get; set; }
 public string? ResultHash { get; set; }
     
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   // Navigation
        public GameSession? Session { get; set; }
        public User? User { get; set; }
    }
}
