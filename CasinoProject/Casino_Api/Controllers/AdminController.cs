using Casino_Api.Data;
using Casino_Api.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Casino_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AppDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardResponse>> GetDashboardData()
    {
        try
        {
            // TODO: Add admin authentication check
            // For now, this is open - should add [Authorize(Roles = "Admin")] attribute

            var dashboard = new AdminDashboardResponse
            {
                UserStats = await GetUserStatisticsData(),
                TransactionStats = await GetTransactionStatisticsData(),
                GameStats = await GetGameStatisticsData()
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin dashboard data");
            return StatusCode(500, new { message = "Error fetching dashboard data" });
        }
    }

    [HttpGet("users")]
    public async Task<ActionResult<UserStatistics>> GetUserStatistics()
    {
        try
        {
            var stats = await GetUserStatisticsData();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user statistics");
            return StatusCode(500, new { message = "Error fetching user statistics" });
        }
    }

    private async Task<UserStatistics> GetUserStatisticsData()
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            var totalUsers = await _context.Users.CountAsync();
            
            // Active users (users who have played in the last 7 days)
            var activeUsers = await _context.Bets
                .Where(b => b.CreatedAt >= weekAgo)
                .Select(b => b.UserId)
                .Distinct()
                .CountAsync();

            var newSignupsToday = await _context.Users
                .CountAsync(u => u.CreatedAt >= today);

            var newSignupsThisWeek = await _context.Users
                .CountAsync(u => u.CreatedAt >= weekAgo);

            var newSignupsThisMonth = await _context.Users
                .CountAsync(u => u.CreatedAt >= monthAgo);

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .Select(u => new UserActivityDto
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Balance = u.Balance,
                    CreatedAt = u.CreatedAt,
                    LastActivityAt = u.Bets.OrderByDescending(b => b.CreatedAt).FirstOrDefault() != null 
                        ? u.Bets.OrderByDescending(b => b.CreatedAt).First().CreatedAt 
                        : null
                })
                .ToListAsync();

            return new UserStatistics
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewSignupsToday = newSignupsToday,
                NewSignupsThisWeek = newSignupsThisWeek,
                NewSignupsThisMonth = newSignupsThisMonth,
                RecentUsers = recentUsers
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserStatisticsData");
            throw;
        }
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<TransactionStatistics>> GetTransactionStatistics()
    {
        try
        {
            var stats = await GetTransactionStatisticsData();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transaction statistics");
            return StatusCode(500, new { message = "Error fetching transaction statistics" });
        }
    }

    private async Task<TransactionStatistics> GetTransactionStatisticsData()
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            var allBets = await _context.Bets
                .Include(b => b.User)
                .ToListAsync();

            var totalVolume = allBets.Sum(b => b.Amount);
            var todayVolume = allBets.Where(b => b.CreatedAt >= today).Sum(b => b.Amount);
            var weekVolume = allBets.Where(b => b.CreatedAt >= weekAgo).Sum(b => b.Amount);
            var monthVolume = allBets.Where(b => b.CreatedAt >= monthAgo).Sum(b => b.Amount);

            var totalTransactions = allBets.Count;
            var todayTransactions = allBets.Count(b => b.CreatedAt >= today);
            var averageTransaction = totalTransactions > 0 ? totalVolume / totalTransactions : 0;

            var recentTransactions = allBets
                .OrderByDescending(b => b.CreatedAt)
                .Take(20)
                .Select(b => new RecentTransactionDto
                {
                    BetId = b.Id,
                    Username = b.User.Username,
                    GameType = b.Game,
                    Amount = b.Amount,
                    WinAmount = b.Payout,
                    CreatedAt = b.CreatedAt
                })
                .ToList();

            return new TransactionStatistics
            {
                TotalVolume = totalVolume,
                TodayVolume = todayVolume,
                WeekVolume = weekVolume,
                MonthVolume = monthVolume,
                TotalTransactions = totalTransactions,
                TodayTransactions = todayTransactions,
                AverageTransactionAmount = averageTransaction,
                RecentTransactions = recentTransactions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetTransactionStatisticsData");
            throw;
        }
    }

    [HttpGet("games")]
    public async Task<ActionResult<GameStatistics>> GetGameStatistics()
    {
        try
        {
            var stats = await GetGameStatisticsData();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching game statistics");
            return StatusCode(500, new { message = "Error fetching game statistics" });
        }
    }

    private async Task<GameStatistics> GetGameStatisticsData()
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            var allBets = await _context.Bets.ToListAsync();

            var totalGames = allBets.Count;
            var gamesToday = allBets.Count(b => b.CreatedAt >= today);
            var gamesThisWeek = allBets.Count(b => b.CreatedAt >= weekAgo);
            var gamesThisMonth = allBets.Count(b => b.CreatedAt >= monthAgo);

            var totalWagered = allBets.Sum(b => b.Amount);
            var totalWon = allBets.Sum(b => b.Payout);
            var houseEdge = totalWagered > 0 ? ((totalWagered - totalWon) / totalWagered) * 100 : 0;

            var gameTypeStats = allBets
                .GroupBy(b => b.Game)
                .Select(g => new GameTypeStats
                {
                    GameType = g.Key,
                    GamesPlayed = g.Count(),
                    TotalWagered = g.Sum(b => b.Amount),
                    TotalWon = g.Sum(b => b.Payout),
                    WinRate = g.Any() ? (decimal)g.Count(b => b.Payout > 0) / g.Count() * 100 : 0
                })
                .ToList();

            return new GameStatistics
            {
                TotalGamesPlayed = totalGames,
                GamesToday = gamesToday,
                GamesThisWeek = gamesThisWeek,
                GamesThisMonth = gamesThisMonth,
                TotalWagered = totalWagered,
                TotalWon = totalWon,
                HouseEdge = houseEdge,
                GameTypeStatistics = gameTypeStats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetGameStatisticsData");
            throw;
        }
    }
}
