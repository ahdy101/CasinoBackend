namespace Casino.Backend.DTOs.Responses
{
    /// <summary>
    /// Response model for bet placement and game results
    /// </summary>
    public class BetResponse
    {
        /// <summary>
    /// Unique identifier for the bet
        /// </summary>
 public int BetId { get; set; }

        /// <summary>
        /// User ID who placed the bet
  /// </summary>
        public int UserId { get; set; }

   /// <summary>
 /// Username of the player
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
     /// Game type (e.g., "Roulette", "Blackjack", "Poker", "Slots")
        /// </summary>
     public string GameType { get; set; } = string.Empty;

        /// <summary>
        /// Bet amount placed
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Player's choice (e.g., "Red", "Black", "17", "Hit", "Stand")
        /// </summary>
        public string Choice { get; set; } = string.Empty;

        /// <summary>
  /// Payout amount (0 if loss)
        /// </summary>
        public decimal Payout { get; set; }

      /// <summary>
        /// Player's new balance after the bet
        /// </summary>
        public decimal NewBalance { get; set; }

  /// <summary>
        /// When the bet was placed
     /// </summary>
    public DateTime PlacedAt { get; set; }
    }
}
