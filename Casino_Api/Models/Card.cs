namespace Casino.Backend.Models
{
    /// <summary>
    /// Represents a playing card
    /// </summary>
public class Card
    {
        /// <summary>
        /// Card suit (Hearts, Diamonds, Clubs, Spades)
        /// </summary>
        public string Suit { get; set; } = string.Empty;

        /// <summary>
        /// Card rank (A, 2-10, J, Q, K)
        /// </summary>
 public string Rank { get; set; } = string.Empty;

   /// <summary>
        /// Numeric value for game calculations
     /// </summary>
      public int Value { get; set; }

public Card(string suit, string rank)
        {
            Suit = suit;
   Rank = rank;
  Value = CalculateValue(rank);
        }

      /// <summary>
        /// Calculate the numeric value of the card
        /// </summary>
        private int CalculateValue(string rank)
      {
         return rank switch
          {
              "A" => 11, // Aces are 11 by default (can be 1 in Blackjack)
       "J" or "Q" or "K" => 10,
         _ => int.TryParse(rank, out int val) ? val : 0
       };
        }

        /// <summary>
        /// String representation with suit symbol
        /// </summary>
        public override string ToString() => $"{Rank}{GetSuitSymbol()}";

        private string GetSuitSymbol()
        {
      return Suit switch
 {
   "Hearts" => "?",
                "Diamonds" => "?",
   "Clubs" => "?",
        "Spades" => "?",
     _ => ""
        };
        }

      /// <summary>
        /// Check if this is an Ace
        /// </summary>
        public bool IsAce() => Rank == "A";

        /// <summary>
        /// Check if this is a face card (J, Q, K)
        /// </summary>
        public bool IsFaceCard() => Rank == "J" || Rank == "Q" || Rank == "K";
    }
}
