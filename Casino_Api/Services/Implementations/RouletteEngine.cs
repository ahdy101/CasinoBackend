using Casino.Backend.Infrastructure;
using Casino.Backend.Services.Interfaces;

namespace Casino.Backend.Services.Implementations
{
    /// <summary>
    /// Roulette game engine implementation (European wheel: 0-36)
    /// </summary>
    public class RouletteEngine : IRouletteEngine
    {
     private readonly IRandomNumberGenerator _rng;
        private readonly ILogger<RouletteEngine> _logger;

        // European roulette red numbers
        private static readonly int[] RedNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
  
        // European roulette black numbers
        private static readonly int[] BlackNumbers = { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 };

        public RouletteEngine(IRandomNumberGenerator rng, ILogger<RouletteEngine> logger)
        {
   _rng = rng;
   _logger = logger;
  }

        /// <summary>
        /// Spin the roulette wheel and process all bets
     /// </summary>
        public async Task<RouletteResult> Spin(int userId, List<RouletteBet> bets)
        {
         _logger.LogInformation("Roulette spin - UserId: {UserId}, Bets: {BetCount}", userId, bets.Count);

            // Generate winning number using cryptographically secure RNG
  int winningNumber = GenerateRandomNumber();
     string winningColor = GetColor(winningNumber);

       var betResults = new List<RouletteBetResult>();
            decimal totalPayout = 0m;

  // Process each bet
    foreach (var bet in bets)
         {
                bool won = IsBetWinner(bet, winningNumber);
                decimal payout = won ? CalculatePayout(bet, winningNumber) : 0m;
      totalPayout += payout;

         betResults.Add(new RouletteBetResult
      {
        BetType = bet.BetType,
     Amount = bet.Amount,
                    Won = won,
           Payout = payout
     });

       _logger.LogInformation("Bet result - Type: {Type}, Won: {Won}, Payout: {Payout}", 
          bet.BetType, won, payout);
      }

       return await Task.FromResult(new RouletteResult
            {
  WinningNumber = winningNumber,
                WinningColor = winningColor,
        BetResults = betResults,
      TotalPayout = totalPayout,
       NewBalance = 0m // Will be set by controller
    });
    }

        /// <summary>
        /// Generate random number (0-36 for European roulette)
      /// </summary>
     public int GenerateRandomNumber()
        {
        return _rng.GetRandomInt(0, 37); // 0-36 inclusive
        }

        /// <summary>
        /// Calculate payout for a specific bet
        /// </summary>
        public decimal CalculatePayout(RouletteBet bet, int winningNumber)
   {
            if (!IsBetWinner(bet, winningNumber))
    return 0m;

            // Payout multipliers based on bet type
       return bet.BetType.ToLower() switch
            {
          "straight" => bet.Amount * 36m,      // 35:1 + original bet
      "split" => bet.Amount * 18m,         // 17:1 + original bet
             "street" => bet.Amount * 12m,        // 11:1 + original bet
        "corner" => bet.Amount * 9m, // 8:1 + original bet
    "sixline" => bet.Amount * 6m,        // 5:1 + original bet
  "dozen" => bet.Amount * 3m,          // 2:1 + original bet
    "column" => bet.Amount * 3m,         // 2:1 + original bet
 "red" => bet.Amount * 2m,        // 1:1 + original bet
   "black" => bet.Amount * 2m,          // 1:1 + original bet
                "even" => bet.Amount * 2m,    // 1:1 + original bet
         "odd" => bet.Amount * 2m,      // 1:1 + original bet
            "low" => bet.Amount * 2m, // 1:1 + original bet (1-18)
     "high" => bet.Amount * 2m, // 1:1 + original bet (19-36)
       _ => 0m
     };
        }

        /// <summary>
        /// Check if a bet wins based on the winning number
        /// </summary>
        public bool IsBetWinner(RouletteBet bet, int winningNumber)
        {
 return bet.BetType.ToLower() switch
            {
  "straight" => int.TryParse(bet.Value, out int num) && num == winningNumber,
            "red" => RedNumbers.Contains(winningNumber),
            "black" => BlackNumbers.Contains(winningNumber),
"even" => winningNumber != 0 && winningNumber % 2 == 0,
    "odd" => winningNumber != 0 && winningNumber % 2 == 1,
          "low" => winningNumber >= 1 && winningNumber <= 18,
 "high" => winningNumber >= 19 && winningNumber <= 36,
           "dozen1" => winningNumber >= 1 && winningNumber <= 12,
         "dozen2" => winningNumber >= 13 && winningNumber <= 24,
   "dozen3" => winningNumber >= 25 && winningNumber <= 36,
            "column1" => winningNumber % 3 == 1,
       "column2" => winningNumber % 3 == 2,
     "column3" => winningNumber % 3 == 0 && winningNumber != 0,
   _ => false
     };
     }

        /// <summary>
        /// Get the color of a number
        /// </summary>
        private string GetColor(int number)
        {
            if (number == 0) return "Green";
         if (RedNumbers.Contains(number)) return "Red";
        if (BlackNumbers.Contains(number)) return "Black";
      return "Unknown";
    }
  }
}
