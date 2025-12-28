using Casino.Backend.DTOs.Responses;
using Casino.Backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Casino.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
     private readonly ILogger<StatsController> _logger;

 public StatsController(IUserRepository userRepository, ILogger<StatsController> logger)
        {
      _userRepository = userRepository;
      _logger = logger;
        }

      /// <summary>
      /// Get count of active users (logged in within last 15 minutes)
        /// </summary>
        [HttpGet("active-users")]
        public async Task<IActionResult> GetActiveUsersCount()
        {
            try
         {
          var activeUsers = await _userRepository.GetActiveUsersAsync(15);
 var count = activeUsers.Count();

        return Ok(new { activeUsers = count, timestamp = DateTime.UtcNow });
            }
    catch (Exception ex)
            {
       _logger.LogError(ex, "Error getting active users count");
  return StatusCode(500, new ErrorResponse { Message = "Error retrieving active users" });
 }
}

        /// <summary>
        /// Get online users list (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
      [HttpGet("online-users")]
      public async Task<IActionResult> GetOnlineUsers()
        {
          try
  {
           var users = await _userRepository.GetUsersByRoleAsync("Player");
    var response = users.Select(u => new
  {
        id = u.Id,
          username = u.Username,
        role = u.Role,
            balance = u.Balance
   });

          return Ok(response);
    }
        catch (Exception ex)
      {
                _logger.LogError(ex, "Error getting online users");
                return StatusCode(500, new ErrorResponse { Message = "Error retrieving online users" });
      }
        }

/// <summary>
  /// Get overall platform statistics (Admin only)
        /// </summary>
  [Authorize(Roles = "Admin")]
        [HttpGet("platform-stats")]
        public async Task<IActionResult> GetPlatformStats([FromServices] IBetRepository betRepository)
        {
    try
         {
     var allBets = await betRepository.GetAllAsync();
     var totalBets = allBets.Count();
 var totalWagered = allBets.Sum(b => b.Amount);
      var totalPayout = allBets.Sum(b => b.Payout);
        var wins = allBets.Count(b => b.Payout > b.Amount);
        var losses = allBets.Count(b => b.Payout == 0);
var houseProfit = totalWagered - totalPayout;

    return Ok(new
      {
           totalBets,
  totalWagered,
     totalPayout,
            wins,
         losses,
               houseProfit,
   timestamp = DateTime.UtcNow
        });
            }
            catch (Exception ex)
  {
                _logger.LogError(ex, "Error getting platform stats");
        return StatusCode(500, new ErrorResponse { Message = "Error retrieving platform stats" });
         }
        }

     /// <summary>
        /// Get user statistics by ID (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
      [HttpGet("user-stats/{userId}")]
 public async Task<IActionResult> GetUserStats(int userId, [FromServices] IBetRepository betRepository)
        {
  try
  {
             var userBets = await betRepository.GetBetsByUserIdAsync(userId);
                var totalBets = userBets.Count();
   var totalWagered = userBets.Sum(b => b.Amount);
     var totalWon = userBets.Sum(b => b.Payout);
  var wins = userBets.Count(b => b.Payout > b.Amount);
         var losses = userBets.Count(b => b.Payout == 0);
             var netProfit = totalWon - totalWagered;

      return Ok(new
      {
      userId,
        totalBets,
            totalWagered,
   totalWon,
   wins,
         losses,
  netProfit,
       winRate = totalBets > 0 ? (double)wins / totalBets * 100 : 0,
         timestamp = DateTime.UtcNow
     });
   }
         catch (Exception ex)
      {
_logger.LogError(ex, "Error getting user stats for user: {UserId}", userId);
     return StatusCode(500, new ErrorResponse { Message = "Error retrieving user stats" });
          }
        }
    }
}
