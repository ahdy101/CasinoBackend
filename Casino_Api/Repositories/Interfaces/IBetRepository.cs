using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
    /// <summary>
/// Repository interface for Bet operations
    /// </summary>
    public interface IBetRepository : IRepository<Bet>
    {
        Task<IEnumerable<Bet>> GetBetsByUserIdAsync(int userId);
        Task<Bet?> GetBetWithUserAsync(int betId);
      Task UpdatePayoutAsync(int betId, decimal payout);
    }
}
