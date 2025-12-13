using Casino.Backend.Models;

namespace Casino.Backend.Services.Interfaces
{
    /// <summary>
    /// Poker game engine interface (Texas Hold'em)
    /// </summary>
    public interface IPokerEngine
    {
        /// <summary>
        /// Initialize a new poker table
        /// </summary>
   Task<PokerTable> InitializeTable(int tableId, PokerGameType gameType, decimal buyIn, int maxPlayers = 9);

        /// <summary>
        /// Deal cards to all players at the table
        /// </summary>
        Task<PokerTable> DealCards(int tableId);

        /// <summary>
        /// Process a player action (fold, call, raise, check)
        /// </summary>
   Task<PokerTable> ProcessAction(int tableId, int userId, PokerAction action, decimal? amount = null);

        /// <summary>
        /// Evaluate a poker hand
        /// </summary>
        Task<HandResult> EvaluateHand(List<Card> holeCards, List<Card> communityCards);

        /// <summary>
   /// Determine winners at showdown
    /// </summary>
     Task<List<PokerWinner>> DetermineWinners(int tableId);
 }

    /// <summary>
    /// Poker game type
    /// </summary>
    public enum PokerGameType
    {
   TexasHoldem,
   Omaha,
   SevenCardStud
    }

    /// <summary>
    /// Poker table state
    /// </summary>
    public class PokerTable
    {
   public int TableId { get; set; }
        public PokerGameType GameType { get; set; }
        public decimal BuyIn { get; set; }
        public int MaxPlayers { get; set; }
        public List<PokerPlayer> Players { get; set; } = new();
     public List<Card> CommunityCards { get; set; } = new();
     public decimal Pot { get; set; }
   public int CurrentPlayerIndex { get; set; }
        public PokerRound CurrentRound { get; set; }
     public PokerTableStatus Status { get; set; }
  }

    /// <summary>
    /// Poker player at table
    /// </summary>
    public class PokerPlayer
    {
 public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
   public List<Card> HoleCards { get; set; } = new();
        public decimal ChipStack { get; set; }
   public decimal CurrentBet { get; set; }
   public bool HasFolded { get; set; }
        public bool IsAllIn { get; set; }
    }

    /// <summary>
    /// Poker betting rounds
    /// </summary>
    public enum PokerRound
    {
  PreFlop,
   Flop,
   Turn,
   River,
        Showdown
    }

    /// <summary>
    /// Poker table status
    /// </summary>
    public enum PokerTableStatus
    {
        Waiting,
   InProgress,
      Completed
    }

    /// <summary>
    /// Hand evaluation result
    /// </summary>
    public class HandResult
    {
   public HandRank Rank { get; set; }
      public string Description { get; set; } = string.Empty;
   public List<Card> BestHand { get; set; } = new();
   public int Score { get; set; }
    }

    /// <summary>
    /// Poker hand rankings
    /// </summary>
    public enum HandRank
{
   HighCard = 1,
   OnePair = 2,
   TwoPair = 3,
        ThreeOfAKind = 4,
   Straight = 5,
        Flush = 6,
        FullHouse = 7,
   FourOfAKind = 8,
   StraightFlush = 9,
        RoyalFlush = 10
    }

    /// <summary>
    /// Poker winner information
    /// </summary>
    public class PokerWinner
    {
   public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
   public HandResult HandResult { get; set; } = new();
   public decimal Winnings { get; set; }
    }
}
