namespace Casino.Backend.Enums
{
    /// <summary>
  /// Blackjack/general game status
  /// </summary>
    public enum GameStatus
    {
        Active,
        PlayerBust,
        DealerBust,
   PlayerBlackjack,
        PlayerWin,
        DealerWin,
        Push
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
    /// Poker-specific action types
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
    /// Poker game type
    /// </summary>
    public enum PokerGameType
    {
        TexasHoldem,
      Omaha,
        SevenCardStud
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
}
