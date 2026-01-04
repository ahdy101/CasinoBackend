using Casino.Backend.DTOs.Requests;
using Casino.Backend.DTOs.Responses;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Casino.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Casino.Backend.Controllers
{
    /// <summary>
    /// Game API for Unity WebGL integration
    /// Handles game sessions, bets, and results
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [EnableRateLimiting("game")]  // Rate limit all game endpoints
    public class GamesController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IBetRepository _betRepository;
        private readonly IWalletService _walletService;
        private readonly ILogger<GamesController> _logger;

        // In-memory session store (use Redis in production)
        private static readonly Dictionary<string, GameSession> _activeSessions = new();

        public GamesController(
          IUserRepository userRepository,
  IBetRepository betRepository,
      IWalletService walletService,
ILogger<GamesController> logger)
    {
            _userRepository = userRepository;
  _betRepository = betRepository;
        _walletService = walletService;
   _logger = logger;
    }

   /// <summary>
        /// Start a new game session - Called by Unity when player enters a game
        /// </summary>
        [HttpPost("session/start")]
        [ProducesResponseType(typeof(GameSessionResponse), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> StartSession([FromBody] StartGameSessionRequest request)
        {
            var userId = GetUserId();
      if (userId == null)
   return Unauthorized(new ErrorResponse { Message = "Invalid token" });

        var user = await _userRepository.GetByIdAsync(userId.Value);
          if (user == null)
return NotFound(new ErrorResponse { Message = "User not found" });

            // Generate unique session token
            var sessionToken = GenerateSessionToken();

            var session = new GameSession
   {
  UserId = userId.Value,
          GameType = request.GameType,
        GameId = request.GameId,
    SessionToken = sessionToken,
            Status = "Active",
              StartedAt = DateTime.UtcNow,
         LastActivityAt = DateTime.UtcNow
   };

 // Store session (use database/Redis in production)
            _activeSessions[sessionToken] = session;

            _logger.LogInformation("Game session started - User: {UserId}, Game: {GameType}/{GameId}",
      userId, request.GameType, request.GameId);

     return Ok(new GameSessionResponse
      {
    Success = true,
   SessionToken = sessionToken,
          GameType = request.GameType,
 GameId = request.GameId,
              Balance = user.Balance,
   MinBet = 1.00m,  // Get from game config
     MaxBet = 1000.00m
   });
        }

        /// <summary>
  /// Get current balance - Called frequently by Unity
   /// </summary>
        [HttpGet("balance")]
   [ProducesResponseType(typeof(GameBalanceResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBalance()
  {
          var userId = GetUserId();
   if (userId == null)
         return Unauthorized(new ErrorResponse { Message = "Invalid token" });

          var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
   return NotFound(new ErrorResponse { Message = "User not found" });

       return Ok(new GameBalanceResponse
     {
         Balance = user.Balance,
     Currency = "USD",
   Timestamp = DateTime.UtcNow
  });
        }

  /// <summary>
   /// Place a bet - Called by Unity before each game round
     /// </summary>
        [HttpPost("bet")]
  [ProducesResponseType(typeof(PlaceBetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
     public async Task<IActionResult> PlaceBet([FromBody] PlaceGameBetRequest request)
        {
 var userId = GetUserId();
    if (userId == null)
     return Unauthorized(new ErrorResponse { Message = "Invalid token" });

      // Validate session
     if (!_activeSessions.TryGetValue(request.SessionToken, out var session))
              return BadRequest(new ErrorResponse { Message = "Invalid or expired session" });

    if (session.UserId != userId.Value)
   return Unauthorized(new ErrorResponse { Message = "Session does not belong to this user" });

     var user = await _userRepository.GetByIdAsync(userId.Value);
       if (user == null)
              return NotFound(new ErrorResponse { Message = "User not found" });

  // Check balance
    if (user.Balance < request.Amount)
      {
                return BadRequest(new PlaceBetResponse
          {
  Success = false,
      Message = "Insufficient balance"
              });
     }

     // Deduct bet amount
            var newBalance = user.Balance - request.Amount;
      await _walletService.UpdateBalanceAsync(userId.Value, newBalance);

        // Generate server seed for provably fair
  var serverSeed = GenerateServerSeed();

    // Update session stats
         session.TotalBet += request.Amount;
            session.LastActivityAt = DateTime.UtcNow;

        _logger.LogInformation("Bet placed - User: {UserId}, Amount: {Amount}, Game: {GameType}",
     userId, request.Amount, session.GameType);

      return Ok(new PlaceBetResponse
    {
    Success = true,
                BetId = 0, // Will be assigned when result is recorded
 Amount = request.Amount,
                NewBalance = newBalance,
        ServerSeed = serverSeed
      });
        }

        /// <summary>
     /// Record game result - Called by Unity after each game round completes
        /// </summary>
        [HttpPost("result")]
[ProducesResponseType(typeof(GameResultResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      public async Task<IActionResult> RecordResult([FromBody] RecordGameResultRequest request)
    {
 var userId = GetUserId();
            if (userId == null)
      return Unauthorized(new ErrorResponse { Message = "Invalid token" });

   // Validate session
      if (!_activeSessions.TryGetValue(request.SessionToken, out var session))
           return BadRequest(new ErrorResponse { Message = "Invalid or expired session" });

  if (session.UserId != userId.Value)
   return Unauthorized(new ErrorResponse { Message = "Session does not belong to this user" });

       var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
     return NotFound(new ErrorResponse { Message = "User not found" });

// Add winnings to balance
   var newBalance = user.Balance + request.WinAmount;
      await _walletService.UpdateBalanceAsync(userId.Value, newBalance);

            // Record the bet
            var bet = new Bet
            {
  UserId = userId.Value,
  Game = session.GameType,
   Amount = request.BetAmount,
            Choice = request.ResultData,
         Payout = request.WinAmount,
        CreatedAt = DateTime.UtcNow
   };
   await _betRepository.AddAsync(bet);

   // Update session stats
          session.TotalWin += request.WinAmount;
       session.RoundsPlayed++;
  session.LastActivityAt = DateTime.UtcNow;

            _logger.LogInformation("Game result - User: {UserId}, Bet: {Bet}, Win: {Win}, Game: {GameType}",
   userId, request.BetAmount, request.WinAmount, session.GameType);

     return Ok(new GameResultResponse
            {
       Success = true,
           BetAmount = request.BetAmount,
     WinAmount = request.WinAmount,
     NewBalance = newBalance
   });
}

        /// <summary>
     /// End game session - Called by Unity when player leaves a game
      /// </summary>
        [HttpPost("session/end")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult EndSession([FromBody] EndGameSessionRequest request)
        {
         var userId = GetUserId();
   if (userId == null)
     return Unauthorized(new ErrorResponse { Message = "Invalid token" });

     if (_activeSessions.TryGetValue(request.SessionToken, out var session))
      {
  if (session.UserId == userId.Value)
 {
      session.Status = "Completed";
         session.EndedAt = DateTime.UtcNow;
   _activeSessions.Remove(request.SessionToken);

          _logger.LogInformation("Game session ended - User: {UserId}, Rounds: {Rounds}, TotalBet: {TotalBet}, TotalWin: {TotalWin}",
     userId, session.RoundsPlayed, session.TotalBet, session.TotalWin);
       }
   }

            return Ok(new { success = true, message = "Session ended" });
   }

        /// <summary>
        /// Get game history for current user
        /// </summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<GameHistoryItemResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistory([FromQuery] string? gameType = null, [FromQuery] int limit = 50)
        {
        var userId = GetUserId();
 if (userId == null)
       return Unauthorized(new ErrorResponse { Message = "Invalid token" });

            var bets = await _betRepository.GetBetsByUserIdAsync(userId.Value);

        var history = bets
         .Where(b => string.IsNullOrEmpty(gameType) || b.Game == gameType)
         .OrderByDescending(b => b.CreatedAt)
      .Take(limit)
     .Select(b => new GameHistoryItemResponse
      {
          Id = b.Id,
      GameType = b.Game,
    BetAmount = b.Amount,
       WinAmount = b.Payout,
ResultData = b.Choice,
          PlayedAt = b.CreatedAt
   })
          .ToList();

 return Ok(history);
        }

      #region Private Helpers

        private int? GetUserId()
   {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
  return null;
        return userId;
        }

        private static string GenerateSessionToken()
        {
         var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

    private static string GenerateServerSeed()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
 return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        #endregion
    }
}
