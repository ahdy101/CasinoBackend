namespace Casino.Backend.Services.Interfaces
{
    /// <summary>
    /// Roulette game engine interface
    /// </summary>
    public interface IRouletteEngine
    {
        /// <summary>
        /// Spin the roulette wheel and process bets
        /// </summary>
        /// <param name="userId">User ID placing bets</param>
        /// <param name="bets">List of bets placed</param>
        /// <returns>Roulette result with winning number and payouts</returns>
      Task<RouletteResult> Spin(int userId, List<RouletteBet> bets);

        /// <summary>
        /// Generate a random roulette number (0-36 for European, 0-37 for American)
   /// </summary>
     /// <returns>Winning number</returns>
        int GenerateRandomNumber();

        /// <summary>
      /// Calculate payout for a specific bet type
        /// </summary>
        /// <param name="bet">Bet information</param>
        /// <param name="winningNumber">The number that won</param>
        /// <returns>Payout amount (0 if loss)</returns>
        decimal CalculatePayout(RouletteBet bet, int winningNumber);

     /// <summary>
  /// Check if a bet wins based on the winning number
        /// </summary>
        bool IsBetWinner(RouletteBet bet, int winningNumber);
    }

    /// <summary>
    /// Result of a roulette spin
    /// </summary>
    public class RouletteResult
    {
      public int WinningNumber { get; set; }
      public string WinningColor { get; set; } = string.Empty;
        public List<RouletteBetResult> BetResults { get; set; } = new();
  public decimal TotalPayout { get; set; }
 public decimal NewBalance { get; set; }
    }

    /// <summary>
    /// Individual bet result
    /// </summary>
    public class RouletteBetResult
    {
     public string BetType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool Won { get; set; }
     public decimal Payout { get; set; }
    }
}
