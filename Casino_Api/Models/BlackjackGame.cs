using System.ComponentModel.DataAnnotations.Schema;

namespace Casino.Backend.Models
{
    /// <summary>
    /// Blackjack game entity for database persistence
    /// </summary>
    public class BlackjackGame
    {
        public int Id { get; set; }
 
        public int UserId { get; set; }
   
     [ForeignKey("UserId")]
  public User? User { get; set; }

        /// <summary>
        /// Original bet amount
   /// </summary>
  [Column(TypeName = "decimal(18,2)")]
public decimal BetAmount { get; set; }

        /// <summary>
        /// Player's cards (serialized as JSON)
        /// </summary>
        public string PlayerCards { get; set; } = "[]";

     /// <summary>
   /// Dealer's cards (serialized as JSON)
     /// </summary>
 public string DealerCards { get; set; } = "[]";

  /// <summary>
/// Current player total
        /// </summary>
        public int PlayerTotal { get; set; }

   /// <summary>
     /// Current dealer total
   /// </summary>
        public int DealerTotal { get; set; }

      /// <summary>
   /// Game status (Active, PlayerBust, DealerBust, PlayerWin, etc.)
    /// </summary>
  public string Status { get; set; } = "Active";

   /// <summary>
        /// Payout amount (null if game not completed)
/// </summary>
   [Column(TypeName = "decimal(18,2)")]
        public decimal? Payout { get; set; }

  /// <summary>
   /// When the game was created
 /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

 /// <summary>
        /// When the game was completed
        /// </summary>
   public DateTime? CompletedAt { get; set; }
    }
}
