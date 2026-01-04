namespace Casino.Backend.DTOs.Responses
{
    /// <summary>
    /// Response sent to Unity when starting a game session
    /// </summary>
    public class GameSessionResponse
    {
        public bool Success { get; set; }
      public string SessionToken { get; set; } = string.Empty;
        public string GameType { get; set; } = string.Empty;
   public string GameId { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal MinBet { get; set; }
        public decimal MaxBet { get; set; }
 public string? Message { get; set; }
    }

    /// <summary>
    /// Response sent to Unity after placing a bet
  /// </summary>
    public class PlaceBetResponse
    {
 public bool Success { get; set; }
        public int BetId { get; set; }
 public decimal Amount { get; set; }
        public decimal NewBalance { get; set; }
        public string? ServerSeed { get; set; } // For provably fair
        public string? Message { get; set; }
    }

    /// <summary>
    /// Response sent to Unity after recording game result
    /// </summary>
    public class GameResultResponse
    {
  public bool Success { get; set; }
        public decimal BetAmount { get; set; }
 public decimal WinAmount { get; set; }
        public decimal NewBalance { get; set; }
      public string? Message { get; set; }
    }

    /// <summary>
    /// Response with user's current balance (called frequently by Unity)
    /// </summary>
    public class GameBalanceResponse
    {
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Game history item for Unity to display
    /// </summary>
    public class GameHistoryItemResponse
    {
      public int Id { get; set; }
      public string GameType { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
  public decimal BetAmount { get; set; }
    public decimal WinAmount { get; set; }
  public string ResultData { get; set; } = string.Empty;
      public DateTime PlayedAt { get; set; }
    }
}
