using Casino.Backend.Infrastructure;
using Casino.Backend.Models;

namespace Casino.Backend.Infrastructure
{
    /// <summary>
    /// Interface for creating and managing card decks
/// </summary>
    public interface ICardDeckFactory
    {
        /// <summary>
        /// Create a new standard 52-card deck
/// </summary>
        List<Card> CreateStandardDeck();

        /// <summary>
        /// Shuffle a deck using cryptographically secure Fisher-Yates algorithm
   /// </summary>
        void Shuffle(List<Card> deck);

        /// <summary>
        /// Create multiple decks (for games like Blackjack that use 6-8 decks)
   /// </summary>
    List<Card> CreateMultipleDecks(int deckCount);
    }

    /// <summary>
    /// Factory for creating and shuffling card decks
    /// ?? CRITICAL: Uses cryptographically secure RNG for fair shuffling
    /// </summary>
    public class CardDeckFactory : ICardDeckFactory
    {
   private readonly IRandomNumberGenerator _rng;

        public CardDeckFactory(IRandomNumberGenerator rng)
     {
            _rng = rng;
    }

        /// <summary>
     /// Create a standard 52-card deck
    /// </summary>
        public List<Card> CreateStandardDeck()
        {
            var deck = new List<Card>();
 var suits = new[] { "Hearts", "Diamonds", "Clubs", "Spades" };
   var ranks = new[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

            foreach (var suit in suits)
         {
    foreach (var rank in ranks)
    {
             deck.Add(new Card(suit, rank));
  }
   }

   return deck;
        }

        /// <summary>
    /// Create multiple decks (e.g., 6 decks for Blackjack)
     /// </summary>
        public List<Card> CreateMultipleDecks(int deckCount)
    {
      var combinedDeck = new List<Card>();
      
  for (int i = 0; i < deckCount; i++)
   {
          combinedDeck.AddRange(CreateStandardDeck());
     }

  return combinedDeck;
   }

   /// <summary>
        /// Shuffle deck using Fisher-Yates algorithm with cryptographically secure RNG
        /// ?? CRITICAL: This is the ONLY acceptable way to shuffle cards in a casino game
     /// </summary>
 public void Shuffle(List<Card> deck)
   {
       int n = deck.Count;
      
       // Fisher-Yates shuffle
       while (n > 1)
            {
       n--;
     int k = _rng.GetRandomInt(0, n + 1);
         
      // Swap deck[k] with deck[n]
    var temp = deck[k];
         deck[k] = deck[n];
    deck[n] = temp;
     }
        }
    }
}
