namespace Casino.Backend.Services.Interfaces
{
    /// <summary>
 /// Master orchestration service for all game engines
    /// </summary>
    public interface IGameEngineService
    {
        /// <summary>
        /// Play a round of Blackjack
        /// </summary>
   Task<GameResult> PlayBlackjack(int userId, PlayerAction action, int? gameId = null, decimal? betAmount = null);

        /// <summary>
        /// Play a spin of Roulette
   /// </summary>
   Task<GameResult> PlayRoulette(int userId, List<RouletteBet> bets);

   /// <summary>
        /// Play a hand of Poker
      /// </summary>
        Task<GameResult> PlayPoker(int tableId, int userId, PokerAction action, decimal? amount = null);

  /// <summary>
        /// Get game history for a user
        /// </summary>
        Task<List<GameHistoryRecord>> GetGameHistory(int userId, string? gameType = null, DateTime? from = null, DateTime? to = null);

        /// <summary>
   /// Get player statistics
  /// </summary>
 Task<PlayerStats> GetPlayerStatistics(int userId);
    }

    /// <summary>
  /// Result of a game operation
    /// </summary>
    public class GameResult
    {
  public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public decimal? Payout { get; set; }
        public decimal? NewBalance { get; set; }
     public List<string> Errors { get; set; } = new();

        public static GameResult Successful(object data, decimal? payout = null, decimal? newBalance = null)
 {
       return new GameResult
{
       Success = true,
       Message = "Game completed successfully",
    Data = data,
       Payout = payout,
           NewBalance = newBalance
      };
        }

        public static GameResult InsufficientFunds()
    {
return new GameResult
{
          Success = false,
                Message = "Insufficient funds",
   Errors = new List<string> { "Your balance is too low to place this bet." }
      };
  }

        public static GameResult InvalidRequest(string reason = "Invalid request")
        {
        return new GameResult
      {
         Success = false,
       Message = reason,
    Errors = new List<string> { reason }
   };
        }

public static GameResult Error(string errorMessage)
   {
       return new GameResult
     {
     Success = false,
       Message = "Game error",
     Errors = new List<string> { errorMessage }
     };
   }
 }

    /// <summary>
    /// Player action types
    /// </summary>
    public enum PlayerAction
    {
  None,
        Hit,
      Stand,
   DoubleDown,
        Split,
   Bet,
 Fold,
   Call,
        Raise
    }

    /// <summary>
  /// Poker action types
    /// </summary>
    public enum PokerAction
    {
        Fold,
  Call,
        Raise,
        Check,
        AllIn
  }

    /// <summary>
    /// Roulette bet information
    /// </summary>
    public class RouletteBet
    {
   public string BetType { get; set; } = string.Empty; // "Straight", "Red", "Black", etc.
   public decimal Amount { get; set; }
   public string Value { get; set; } = string.Empty; // Number or color
    }

 /// <summary>
   /// Game history record
    /// </summary>
    public class GameHistoryRecord
 {
        public int BetId { get; set; }
        public string GameType { get; set; } = string.Empty;
   public decimal Amount { get; set; }
  public string Choice { get; set; } = string.Empty;
   public decimal Payout { get; set; }
    public DateTime PlayedAt { get; set; }
    }

    /// <summary>
    /// Player statistics
    /// </summary>
    public class PlayerStats
    {
   public int TotalGames { get; set; }
 public decimal TotalWagered { get; set; }
        public decimal TotalWon { get; set; }
   public decimal NetProfit { get; set; }
        public decimal WinRate { get; set; }
        public string FavoriteGame { get; set; } = string.Empty;
    }
}
