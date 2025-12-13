using Casino.Backend.Data;
using Casino.Backend.DTOs.Responses;
using Casino.Backend.Infrastructure;
using Casino.Backend.Models;
using Casino.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Casino.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IRouletteEngine _rouletteEngine;
        private readonly IWalletService _walletService;
        private readonly ILogger<GameController> _logger;

        public GameController(
            AppDbContext db,
            IRouletteEngine rouletteEngine,
            IWalletService walletService,
            ILogger<GameController> logger)
        {
            _db = db;
            _rouletteEngine = rouletteEngine;
            _walletService = walletService;
            _logger = logger;
        }

        private bool IsApiKeyValid(string apiKey)
        {
            return !string.IsNullOrEmpty(apiKey) && _db.TenantApiKeys.Any(k => k.ApiKey == apiKey);
        }

        /// <summary>
        /// Legacy roulette endpoint - redirects to new RouletteController
        /// </summary>
        [HttpPost("roulette")]
        [Authorize]
        [Obsolete("Use /api/roulette/spin instead")]
        public async Task<IActionResult> PlayRoulette([FromBody] RouletteRequest req, [FromQuery] string apiKey)
        {
            if (!IsApiKeyValid(apiKey))
                return Unauthorized(new ErrorResponse { Message = "Invalid or missing API key." });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Deduct bet
            var walletResult = await _walletService.DeductBet(userId, req.Amount, "Roulette", req.Choice);
            if (!walletResult.Success)
            {
                return Conflict(new ErrorResponse { Message = walletResult.Message, Errors = walletResult.Errors });
            }

            try
            {
                // Create single bet
                var bet = new RouletteBet { BetType = "straight", Amount = req.Amount, Value = req.Choice };
                var result = await _rouletteEngine.Spin(userId, new List<RouletteBet> { bet });

                // Process payout
                if (result.TotalPayout > 0 && walletResult.BetId.HasValue)
                {
                    await _walletService.ProcessPayout(userId, walletResult.BetId.Value, result.TotalPayout);
                }

                var newBalance = await _walletService.GetBalance(userId);

                return Ok(new
                {
                    roll = result.WinningNumber,
                    payout = result.TotalPayout,
                    balance = newBalance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in legacy roulette endpoint");
                return StatusCode(500, new ErrorResponse { Message = "An error occurred" });
            }
        }
    }
}
