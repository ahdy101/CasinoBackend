using Casino_Api.Models;

namespace Casino_Api.Repositories.Interfaces;

public interface IBlackjackGameRepository : IRepository<BlackjackGame>
{
    Task<IEnumerable<BlackjackGame>> GetGamesByUserIdAsync(int userId);
    Task<BlackjackGame?> GetActiveGameAsync(int userId);
    Task<IEnumerable<BlackjackGame>> GetCompletedGamesAsync(int userId, int limit = 10);
}
