using Casino.Backend.Models;

namespace Casino.Backend.Services.Interfaces
{
    /// <summary>
    /// Blackjack game engine interface
    /// </summary>
    public interface IBlackjackEngine
    {
        /// <summary>
        /// Initialize a new Blackjack game
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="betAmount">Bet amount</param>
        /// <returns>Initial game state</returns>
        Task<BlackjackGameState> InitializeGame(int userId, decimal betAmount);

        /// <summary>
        /// Player hits (draws a card)
        /// </summary>
        Task<BlackjackGameState> Hit(int gameId, int userId);

        /// <summary>
        /// Player stands (ends their turn)
        /// </summary>
        Task<BlackjackGameState> Stand(int gameId, int userId);

        /// <summary>
        /// Player doubles down (double bet, one card, then stand)
        /// </summary>
        Task<BlackjackGameState> DoubleDown(int gameId, int userId);

        /// <summary>
        /// Player splits pairs (creates two hands)
        /// </summary>
        Task<BlackjackGameState> Split(int gameId, int userId);

        /// <summary>
        /// Calculate payout based on game outcome
        /// </summary>
        decimal CalculatePayout(BlackjackGameState state);
    }

    /// <summary>
    /// Blackjack game state
    /// </summary>
    public class BlackjackGameState
    {
        public int GameId { get; set; }
        public List<Card> PlayerHand { get; set; } = new();
        public List<Card> DealerHand { get; set; } = new();
        public int PlayerTotal { get; set; }
        public int DealerTotal { get; set; }
        public bool DealerShowsAll { get; set; }
        public GameStatus Status { get; set; }
        public decimal BetAmount { get; set; }
        public decimal? Payout { get; set; }
        public bool CanHit { get; set; }
        public bool CanStand { get; set; }
        public bool CanDoubleDown { get; set; }
        public bool CanSplit { get; set; }
    }

    /// <summary>
    /// Game status
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
}
