using Casino_Api.Data;
using Casino_Api.DTOs.Requests;
using Casino_Api.DTOs.Responses;
using Casino_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Casino_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameStateController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<GameStateController> _logger;

    public GameStateController(AppDbContext context, ILogger<GameStateController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveGameState([FromBody] SaveGameStateRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var existingState = await _context.GameStates
                .FirstOrDefaultAsync(gs => gs.UserId == userId && gs.GameType == request.GameType);

            if (existingState != null)
            {
                existingState.State = request.State;
                existingState.UpdatedAt = DateTime.UtcNow;
                _context.GameStates.Update(existingState);
            }
            else
            {
                var newState = new GameState
                {
                    UserId = userId,
                    GameType = request.GameType,
                    State = request.State,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.GameStates.AddAsync(newState);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Game state saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving game state");
            return StatusCode(500, new { message = "Error saving game state" });
        }
    }

    [HttpGet("load/{gameType}")]
    public async Task<ActionResult<GameStateResponse>> LoadGameState(string gameType)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var gameState = await _context.GameStates
                .FirstOrDefaultAsync(gs => gs.UserId == userId && gs.GameType == gameType);

            if (gameState == null)
            {
                return NotFound(new { message = "No saved game state found" });
            }

            var response = new GameStateResponse
            {
                Id = gameState.Id,
                GameType = gameState.GameType,
                State = gameState.State,
                CreatedAt = gameState.CreatedAt,
                UpdatedAt = gameState.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading game state");
            return StatusCode(500, new { message = "Error loading game state" });
        }
    }

    [HttpDelete("{gameType}")]
    public async Task<IActionResult> DeleteGameState(string gameType)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var gameState = await _context.GameStates
                .FirstOrDefaultAsync(gs => gs.UserId == userId && gs.GameType == gameType);

            if (gameState != null)
            {
                _context.GameStates.Remove(gameState);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Game state deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game state");
            return StatusCode(500, new { message = "Error deleting game state" });
        }
    }

    [HttpGet("has/{gameType}")]
    public async Task<ActionResult<bool>> HasSavedGame(string gameType)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var exists = await _context.GameStates
                .AnyAsync(gs => gs.UserId == userId && gs.GameType == gameType);

            return Ok(new { hasSavedGame = exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for saved game");
            return StatusCode(500, new { message = "Error checking for saved game" });
        }
    }

    [HttpPost("stats")]
    public async Task<IActionResult> UpdateGameStats([FromBody] UpdateGameStatsRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var stats = await _context.GameStatistics
                .FirstOrDefaultAsync(gs => gs.UserId == userId && gs.GameType == request.GameType);

            if (stats == null)
            {
                stats = new Models.GameStatistics
                {
                    UserId = userId,
                    GameType = request.GameType,
                    Wins = 0,
                    Losses = 0,
                    Pushes = 0,
                    TotalWagered = 0,
                    TotalWon = 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.GameStatistics.AddAsync(stats);
            }

            // Update based on result
            switch (request.Result.ToLower())
            {
                case "win":
                    stats.Wins++;
                    break;
                case "loss":
                    stats.Losses++;
                    break;
                case "push":
                    stats.Pushes++;
                    break;
            }

            stats.TotalWagered += request.Wagered;
            stats.TotalWon += request.Won;
            stats.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Game statistics updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating game statistics");
            return StatusCode(500, new { message = "Error updating game statistics" });
        }
    }

    [HttpGet("stats/{gameType}")]
    public async Task<ActionResult<GameStatsResponse>> GetGameStats(string gameType)
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var stats = await _context.GameStatistics
                .FirstOrDefaultAsync(gs => gs.UserId == userId && gs.GameType == gameType);

            if (stats == null)
            {
                return Ok(new GameStatsResponse
                {
                    GameType = gameType,
                    Wins = 0,
                    Losses = 0,
                    Pushes = 0,
                    TotalWagered = 0,
                    TotalWon = 0,
                    WinRate = 0,
                    Profit = 0
                });
            }

            var totalGames = stats.Wins + stats.Losses + stats.Pushes;
            var winRate = totalGames > 0 ? (decimal)stats.Wins / totalGames * 100 : 0;
            var profit = stats.TotalWon - stats.TotalWagered;

            return Ok(new GameStatsResponse
            {
                GameType = stats.GameType,
                Wins = stats.Wins,
                Losses = stats.Losses,
                Pushes = stats.Pushes,
                TotalWagered = stats.TotalWagered,
                TotalWon = stats.TotalWon,
                WinRate = winRate,
                Profit = profit
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching game statistics");
            return StatusCode(500, new { message = "Error fetching game statistics" });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AllGameStatsResponse>> GetAllGameStats()
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var allStats = await _context.GameStatistics
                .Where(gs => gs.UserId == userId)
                .ToListAsync();

            var response = new AllGameStatsResponse
            {
                Stats = allStats.Select(stats =>
                {
                    var totalGames = stats.Wins + stats.Losses + stats.Pushes;
                    var winRate = totalGames > 0 ? (decimal)stats.Wins / totalGames * 100 : 0;
                    var profit = stats.TotalWon - stats.TotalWagered;

                    return new GameStatsResponse
                    {
                        GameType = stats.GameType,
                        Wins = stats.Wins,
                        Losses = stats.Losses,
                        Pushes = stats.Pushes,
                        TotalWagered = stats.TotalWagered,
                        TotalWon = stats.TotalWon,
                        WinRate = winRate,
                        Profit = profit
                    };
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all game statistics");
            return StatusCode(500, new { message = "Error fetching all game statistics" });
        }
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<GameStateResponse>>> GetAllSavedGames()
    {
        try
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var savedGames = await _context.GameStates
                .Where(gs => gs.UserId == userId)
                .Select(gs => new GameStateResponse
                {
                    Id = gs.Id,
                    GameType = gs.GameType,
                    State = gs.State,
                    CreatedAt = gs.CreatedAt,
                    UpdatedAt = gs.UpdatedAt
                })
                .ToListAsync();

            return Ok(savedGames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all saved games");
            return StatusCode(500, new { message = "Error fetching all saved games" });
        }
    }
}
