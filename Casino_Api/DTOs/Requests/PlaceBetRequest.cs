using System.ComponentModel.DataAnnotations;

namespace Casino.Backend.DTOs.Requests
{
    /// <summary>
    /// Request model for placing a bet
    /// </summary>
    public class PlaceBetRequest
    {
        /// <summary>
        /// Game type (e.g., "Roulette", "Blackjack", "Poker", "Slots")
        /// </summary>
        [Required(ErrorMessage = "Game type is required")]
        [MaxLength(50)]
     public string GameType { get; set; } = string.Empty;

        /// <summary>
      /// Bet amount (must be greater than 0 and less than user balance)
     /// </summary>
        [Required]
        [Range(1, 100000, ErrorMessage = "Bet amount must be between 1 and 100,000")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Player's choice (e.g., "Red", "Black", "17", "Hit", "Stand")
        /// </summary>
        [Required(ErrorMessage = "Choice is required")]
        [MaxLength(100)]
        public string Choice { get; set; } = string.Empty;
    }
}
