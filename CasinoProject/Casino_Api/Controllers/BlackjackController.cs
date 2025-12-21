using Casino_Api.DTOs.Requests;
using Casino_Api.DTOs.Responses;
using Casino_Api.Security;
using Casino_Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Casino_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlackjackController : ControllerBase
{
    private readonly IBlackjackEngine _blackjackEngine;
    private readonly TokenValidator _tokenValidator;

    public BlackjackController(
        IBlackjackEngine blackjackEngine,
        TokenValidator tokenValidator)
    {
        _blackjackEngine = blackjackEngine;
        _tokenValidator = tokenValidator;
    }

    /// <summary>
    /// Start a new blackjack game (Requires: Bearer token only)
    /// </summary>
    [HttpPost("deal")]
    [ProducesResponseType(typeof(BlackjackGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deal(
        [FromHeader(Name = "Authorization")] string? authorization,
   [FromQuery] decimal betAmount)
    {
        try
        {
      // Fallback to Request.Headers if parameter is empty
if (string.IsNullOrEmpty(authorization))
       {
        authorization = Request.Headers["Authorization"].FirstOrDefault();
       }

     if (string.IsNullOrEmpty(authorization))
   return Unauthorized(new ErrorResponse("Authorization header is required. Format: Bearer {token}"));

  var validation = _tokenValidator.ValidateToken(authorization);
          if (!validation.IsValid)
    return Unauthorized(new ErrorResponse(validation.Error));

   var game = await _blackjackEngine.InitializeGame(validation.UserId, betAmount);

      var response = new BlackjackGameResponse
 {
       GameId = game.Id,
    PlayerHand = JsonSerializer.Deserialize<List<Models.Card>>(game.PlayerHandJson) ?? new(),
  DealerHand = JsonSerializer.Deserialize<List<Models.Card>>(game.DealerHandJson) ?? new(),
                PlayerTotal = game.PlayerTotal,
   DealerTotal = game.DealerTotal,
    Status = game.Status,
              BetAmount = game.BetAmount,
    Payout = game.Payout,
      CanHit = game.Status == "Active",
      CanStand = game.Status == "Active",
  CanDoubleDown = game.Status == "Active",
    CanSplit = false
   };

      return Ok(response);
 }
        catch (Exception ex)
   {
      return BadRequest(new ErrorResponse(ex.Message));
  }
    }

  /// <summary>
    /// Hit - draw another card (Requires: Bearer token only)
    /// </summary>
    [HttpPost("hit")]
    [ProducesResponseType(typeof(BlackjackGameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Hit(
        [FromHeader(Name = "Authorization")] string? authorization,
      [FromQuery] int gameId)
    {
     try
 {
            // Fallback to Request.Headers if parameter is empty
       if (string.IsNullOrEmpty(authorization))
       {
          authorization = Request.Headers["Authorization"].FirstOrDefault();
    }

    if (string.IsNullOrEmpty(authorization))
 return Unauthorized(new ErrorResponse("Authorization header is required. Format: Bearer {token}"));

    var validation = _tokenValidator.ValidateToken(authorization);
     if (!validation.IsValid)
     return Unauthorized(new ErrorResponse(validation.Error));

       var game = await _blackjackEngine.Hit(gameId, validation.UserId);

        var response = new BlackjackGameResponse
 {
       GameId = game.Id,
      PlayerHand = JsonSerializer.Deserialize<List<Models.Card>>(game.PlayerHandJson) ?? new(),
  DealerHand = JsonSerializer.Deserialize<List<Models.Card>>(game.DealerHandJson) ?? new(),
   PlayerTotal = game.PlayerTotal,
   DealerTotal = game.DealerTotal,
       Status = game.Status,
      BetAmount = game.BetAmount,
       Payout = game.Payout,
     CanHit = game.Status == "Active",
              CanStand = game.Status == "Active",
      CanDoubleDown = false,
     CanSplit = false
     };

       return Ok(response);
        }
        catch (Exception ex)
 {
   return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    /// <summary>
 /// Stand - end player's turn (Requires: Bearer token only)
    /// </summary>
    [HttpPost("stand")]
    [ProducesResponseType(typeof(BlackjackGameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Stand(
        [FromHeader(Name = "Authorization")] string? authorization,
        [FromQuery] int gameId)
    {
        try
        {
     // Fallback to Request.Headers if parameter is empty
      if (string.IsNullOrEmpty(authorization))
            {
       authorization = Request.Headers["Authorization"].FirstOrDefault();
      }

   if (string.IsNullOrEmpty(authorization))
     return Unauthorized(new ErrorResponse("Authorization header is required. Format: Bearer {token}"));

            var validation = _tokenValidator.ValidateToken(authorization);
  if (!validation.IsValid)
    return Unauthorized(new ErrorResponse(validation.Error));

   var game = await _blackjackEngine.Stand(gameId, validation.UserId);

var response = new BlackjackGameResponse
       {
     GameId = game.Id,
       PlayerHand = JsonSerializer.Deserialize<List<Models.Card>>(game.PlayerHandJson) ?? new(),
  DealerHand = JsonSerializer.Deserialize<List<Models.Card>>(game.DealerHandJson) ?? new(),
 PlayerTotal = game.PlayerTotal,
       DealerTotal = game.DealerTotal,
  Status = game.Status,
      BetAmount = game.BetAmount,
     Payout = game.Payout,
     CanHit = false,
   CanStand = false,
   CanDoubleDown = false,
  CanSplit = false
  };

     return Ok(response);
 }
        catch (Exception ex)
        {
   return BadRequest(new ErrorResponse(ex.Message));
   }
    }

    /// <summary>
    /// Double down - double bet and take one card (Requires: Bearer token only)
    /// </summary>
    [HttpPost("double-down")]
  [ProducesResponseType(typeof(BlackjackGameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DoubleDown(
        [FromHeader(Name = "Authorization")] string? authorization,
        [FromQuery] int gameId)
    {
        try
{
         // Fallback to Request.Headers if parameter is empty
  if (string.IsNullOrEmpty(authorization))
      {
       authorization = Request.Headers["Authorization"].FirstOrDefault();
  }

   if (string.IsNullOrEmpty(authorization))
     return Unauthorized(new ErrorResponse("Authorization header is required. Format: Bearer {token}"));

      var validation = _tokenValidator.ValidateToken(authorization);
         if (!validation.IsValid)
      return Unauthorized(new ErrorResponse(validation.Error));

       var game = await _blackjackEngine.DoubleDown(gameId, validation.UserId);

      var response = new BlackjackGameResponse
      {
     GameId = game.Id,
PlayerHand = JsonSerializer.Deserialize<List<Models.Card>>(game.PlayerHandJson) ?? new(),
     DealerHand = JsonSerializer.Deserialize<List<Models.Card>>(game.DealerHandJson) ?? new(),
       PlayerTotal = game.PlayerTotal,
     DealerTotal = game.DealerTotal,
     Status = game.Status,
  BetAmount = game.BetAmount,
   Payout = game.Payout,
             CanHit = false,
   CanStand = false,
   CanDoubleDown = false,
     CanSplit = false
       };

  return Ok(response);
        }
        catch (Exception ex)
   {
     return BadRequest(new ErrorResponse(ex.Message));
 }
    }
}
