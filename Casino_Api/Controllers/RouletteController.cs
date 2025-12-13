using Casino.Backend.Data;
using Casino.Backend.DTOs.Responses;
using Casino.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Casino.Backend.Controllers
{
    /// <summary>
    /// Roulette game controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RouletteController : ControllerBase
    {
        private readonly AppDbContext _db;
    private readonly IRouletteEngine _rouletteEngine;
        private readonly IWalletService _walletService;
        private readonly ILogger<RouletteController> _logger;

        public RouletteController(
 AppDbContext db,
            IRouletteEngine rouletteEngine,
   IWalletService walletService,
            ILogger<RouletteController> logger)
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
        /// Spin the roulette wheel with one or more bets
        /// </summary>
        [HttpPost("spin")]
        [Authorize]
[ProducesResponseType(typeof(RouletteResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
     [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Spin([FromBody] RouletteSpinRequest request, [FromQuery] string apiKey)
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

            // Calculate total bet amount
  decimal totalBetAmount = request.Bets.Sum(b => b.Amount);

            // Deduct total bet from wallet
     var walletResult = await _walletService.DeductBet(userId, totalBetAmount, "Roulette", 
      string.Join(", ", request.Bets.Select(b => $"{b.BetType}:{b.Value}")));

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
      // Spin the wheel
         var result = await _rouletteEngine.Spin(userId, request.Bets);

    // Process payout
       if (result.TotalPayout > 0 && walletResult.BetId.HasValue)
   {
            await _walletService.ProcessPayout(userId, walletResult.BetId.Value, result.TotalPayout);
   }

          // Update result with new balance
         var newBalance = await _walletService.GetBalance(userId);
         result.NewBalance = newBalance;

           _logger.LogInformation("Roulette spin complete - UserId: {UserId}, WinningNumber: {Number}, Payout: {Payout}", 
      userId, result.WinningNumber, result.TotalPayout);

 return Ok(result);
   }
            catch (Exception ex)
            {
 _logger.LogError(ex, "Error spinning roulette for user {UserId}", userId);
                return StatusCode(500, new ErrorResponse
          {
            Message = "An error occurred while spinning roulette",
      Errors = new List<string> { ex.Message }
 });
       }
        }
    }

    /// <summary>
    /// Request model for roulette spin
    /// </summary>
    public class RouletteSpinRequest
    {
/// <summary>
        /// List of bets to place on this spin
        /// </summary>
  [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(1, ErrorMessage = "At least one bet is required")]
  public List<RouletteBet> Bets { get; set; } = new();
    }
}
