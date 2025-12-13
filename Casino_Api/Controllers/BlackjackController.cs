using Casino.Backend.Data;
using Casino.Backend.DTOs.Responses;
using Casino.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Casino.Backend.Controllers
{
    /// <summary>
    /// Blackjack game controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BlackjackController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IBlackjackEngine _blackjackEngine;
        private readonly IWalletService _walletService;
        private readonly ILogger<BlackjackController> _logger;

        public BlackjackController(
            AppDbContext db,
            IBlackjackEngine blackjackEngine,
            IWalletService walletService,
            ILogger<BlackjackController> logger)
        {
            _db = db;
            _blackjackEngine = blackjackEngine;
            _walletService = walletService;
            _logger = logger;
        }

        private bool IsApiKeyValid(string apiKey)
        {
            return !string.IsNullOrEmpty(apiKey) && _db.TenantApiKeys.Any(k => k.ApiKey == apiKey);
        }

        /// <summary>
        /// Deal initial cards and start a new Blackjack game
        /// </summary>
        [HttpPost("deal")]
        [Authorize]
        [ProducesResponseType(typeof(BlackjackGameState), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Deal([FromBody] DealRequest request, [FromQuery] string apiKey)
        {
            if (!IsApiKeyValid(apiKey))
                return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Deduct bet from wallet
            var walletResult = await _walletService.DeductBet(userId, request.BetAmount, "Blackjack", "Deal");
            if (!walletResult.Success)
            {
                return Conflict(new ErrorResponse
                {
                    Message = walletResult.Message,
                    Errors = walletResult.Errors
                });
            }

            try
            {
                // Initialize Blackjack game
                var gameState = await _blackjackEngine.InitializeGame(userId, request.BetAmount);

                // If game completed immediately (blackjack), process payout
                if (gameState.Status != GameStatus.Active && gameState.Payout.HasValue)
                {
                    await _walletService.ProcessPayout(userId, walletResult.BetId!.Value, gameState.Payout.Value);
                    gameState.CanHit = false;
                    gameState.CanStand = false;
                    gameState.CanDoubleDown = false;
                }

                return Ok(gameState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dealing Blackjack game for user {UserId}", userId);
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while dealing cards",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Hit - Draw another card
        /// </summary>
        [HttpPost("hit")]
        [Authorize]
        [ProducesResponseType(typeof(BlackjackGameState), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Hit([FromBody] GameActionRequest request, [FromQuery] string apiKey)
        {
            if (!IsApiKeyValid(apiKey))
                return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                var gameState = await _blackjackEngine.Hit(request.GameId, userId);

                // If game completed (bust), process payout (which is 0)
                if (gameState.Status == GameStatus.PlayerBust && gameState.Payout.HasValue)
                {
                    // Payout is 0, but we still record the transaction
                    // The bet was already deducted, so no wallet operation needed
                }

                return Ok(gameState);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Hit action for user {UserId}, game {GameId}", userId, request.GameId);
                return StatusCode(500, new ErrorResponse { Message = "An error occurred", Errors = new List<string> { ex.Message } });
            }
        }

        /// <summary>
        /// Stand - End player turn, dealer plays
        /// </summary>
        [HttpPost("stand")]
        [Authorize]
        [ProducesResponseType(typeof(BlackjackGameState), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Stand([FromBody] GameActionRequest request, [FromQuery] string apiKey)
        {
            if (!IsApiKeyValid(apiKey))
                return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                var gameState = await _blackjackEngine.Stand(request.GameId, userId);

                // Process payout
                if (gameState.Payout.HasValue)
                {
                    // Find the original bet
                    var bet = await _db.Bets.FirstOrDefaultAsync(b => b.UserId == userId && b.Game == "Blackjack");
                    if (bet != null)
                    {
                        await _walletService.ProcessPayout(userId, bet.Id, gameState.Payout.Value);
                    }
                }

                return Ok(gameState);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Stand action for user {UserId}, game {GameId}", userId, request.GameId);
                return StatusCode(500, new ErrorResponse { Message = "An error occurred", Errors = new List<string> { ex.Message } });
            }
        }

        /// <summary>
        /// Double Down - Double bet, draw one card, then stand
        /// </summary>
        [HttpPost("doubledown")]
        [Authorize]
        [ProducesResponseType(typeof(BlackjackGameState), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DoubleDown([FromBody] GameActionRequest request, [FromQuery] string apiKey)
        {
            if (!IsApiKeyValid(apiKey))
                return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                var gameState = await _blackjackEngine.DoubleDown(request.GameId, userId);

                // Process payout if game completed
                if (gameState.Payout.HasValue)
                {
                    var bet = await _db.Bets.FirstOrDefaultAsync(b => b.UserId == userId && b.Game == "Blackjack");
                    if (bet != null)
                    {
                        await _walletService.ProcessPayout(userId, bet.Id, gameState.Payout.Value);
                    }
                }

                return Ok(gameState);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DoubleDown action for user {UserId}, game {GameId}", userId, request.GameId);
                return StatusCode(500, new ErrorResponse { Message = "An error occurred", Errors = new List<string> { ex.Message } });
            }
        }
    }

    /// <summary>
    /// Request model for dealing a new Blackjack game
    /// </summary>
    public class DealRequest
    {
        /// <summary>
        /// Amount to bet on this hand (must be greater than 0 and less than user balance)
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(1, 100000)]
        public decimal BetAmount { get; set; }
    }

    /// <summary>
    /// Request model for game actions (hit, stand, doubledown)
    /// </summary>
    public class GameActionRequest
    {
        /// <summary>
        /// Game ID to perform action on
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        public int GameId { get; set; }
    }
}
