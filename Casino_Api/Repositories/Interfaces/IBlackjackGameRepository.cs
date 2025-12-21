using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for BlackjackGame operations
    /// </summary>
    public interface IBlackjackGameRepository : IRepository<BlackjackGame>
    {
        Task<IEnumerable<BlackjackGame>> GetGamesByUserIdAsync(int userId);
        Task<BlackjackGame?> GetActiveGameAsync(int userId);
        Task<IEnumerable<BlackjackGame>> GetCompletedGamesAsync(int userId, int limit = 10);
    }
}
