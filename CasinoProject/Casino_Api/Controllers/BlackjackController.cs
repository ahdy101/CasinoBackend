using Casino_Api.DTOs.Requests;
using Casino_Api.DTOs.Responses;
using Casino_Api.Security;
using Casino_Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Casino_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ServiceFilter(typeof(RequireApiKeyAttribute))]
public class BlackjackController : ControllerBase
{
    private readonly IBlackjackEngine _blackjackEngine;

    public BlackjackController(IBlackjackEngine blackjackEngine)
    {
        _blackjackEngine = blackjackEngine;
    }

    /// <summary>
    /// Start a new blackjack game
    /// </summary>
    [HttpPost("deal")]
    [ProducesResponseType(typeof(BlackjackGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deal([FromBody] DealRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var game = await _blackjackEngine.InitializeGame(userId, request.BetAmount);

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
    /// Hit - draw another card
    /// </summary>
    [HttpPost("hit")]
    [ProducesResponseType(typeof(BlackjackGameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Hit([FromBody] HitRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var game = await _blackjackEngine.Hit(request.GameId, userId);

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
    /// Stand - end player's turn
    /// </summary>
    [HttpPost("stand")]
    [ProducesResponseType(typeof(BlackjackGameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Stand([FromBody] StandRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var game = await _blackjackEngine.Stand(request.GameId, userId);

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
    /// Double down - double bet and take one card
    /// </summary>
    [HttpPost("double-down")]
    [ProducesResponseType(typeof(BlackjackGameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DoubleDown([FromBody] DoubleDownRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var game = await _blackjackEngine.DoubleDown(request.GameId, userId);

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
