using System.ComponentModel.DataAnnotations;

namespace Casino.Backend.DTOs.Requests
{
    /// <summary>
    /// Request from Unity to start a game session
    /// </summary>
    public class StartGameSessionRequest
    {
        [Required]
        public string GameType { get; set; } = string.Empty; // "slots", "roulette", "blackjack", "poker"
        
        [Required]
        public string GameId { get; set; } = string.Empty; // Specific game identifier
    }

 /// <summary>
    /// Request from Unity to place a bet
    /// </summary>
    public class PlaceGameBetRequest
    {
        [Required]
        public string SessionToken { get; set; } = string.Empty;
      
        [Required]
        [Range(0.01, 100000)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Game-specific bet data (JSON from Unity)
     /// e.g., slot paylines, roulette numbers, blackjack action
        /// </summary>
  public string? BetData { get; set; }
    }

    /// <summary>
    /// Request from Unity to record game result
 /// </summary>
    public class RecordGameResultRequest
    {
   [Required]
        public string SessionToken { get; set; } = string.Empty;
 
        [Required]
public decimal BetAmount { get; set; }
        
      [Required]
        public decimal WinAmount { get; set; }
        
        /// <summary>
        /// Game result data from Unity (JSON)
        /// e.g., slot reels result, roulette number, cards dealt
        /// </summary>
        [Required]
        public string ResultData { get; set; } = string.Empty;
        
 /// <summary>
      /// Hash for provably fair verification
        /// </summary>
     public string? ResultHash { get; set; }
 }

    /// <summary>
    /// Request from Unity to end game session
    /// </summary>
    public class EndGameSessionRequest
    {
        [Required]
    public string SessionToken { get; set; } = string.Empty;
    }
}
