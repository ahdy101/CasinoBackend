using Casino_Api.Data;
using Casino_Api.Models;
using Casino_Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Casino_Api.Repositories.Implementations;

public class BlackjackGameRepository : Repository<BlackjackGame>, IBlackjackGameRepository
{
    public BlackjackGameRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BlackjackGame>> GetGamesByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<BlackjackGame?> GetActiveGameAsync(int userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(g => g.UserId == userId && g.Status == "Active");
    }

    public async Task<IEnumerable<BlackjackGame>> GetCompletedGamesAsync(int userId, int limit = 10)
    {
        return await _dbSet
            .Where(g => g.UserId == userId && g.CompletedAt != null)
            .OrderByDescending(g => g.CompletedAt)
            .Take(limit)
            .ToListAsync();
    }
}
