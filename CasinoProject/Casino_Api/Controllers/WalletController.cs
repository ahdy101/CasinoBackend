using Casino_Api.DTOs.Requests;
using Casino_Api.DTOs.Responses;
using Casino_Api.Security;
using Casino_Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ServiceFilter(typeof(RequireApiKeyAttribute))]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Get user balance
    /// </summary>
    [HttpGet("balance")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _walletService.GetBalance(userId);

        if (!result.Success)
            return NotFound(new ErrorResponse("User not found"));

        return Ok(new { balance = result.Balance });
    }

    /// <summary>
    /// Add funds to wallet
    /// </summary>
    [HttpPost("add-funds")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddFunds([FromBody] AddFundsRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _walletService.AddFunds(userId, request.Amount);

        if (!result.Success)
            return BadRequest(new ErrorResponse(result.Message));

        return Ok(new { 
            success = true,
            newBalance = result.NewBalance,
            message = result.Message
        });
    }

    /// <summary>
    /// Cash out from wallet
    /// </summary>
    [HttpPost("cash-out")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CashOut([FromBody] CashOutRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _walletService.CashOut(userId, request.Amount);

        if (!result.Success)
            return BadRequest(new ErrorResponse(result.Message));

        return Ok(new { 
            success = true,
            newBalance = result.NewBalance,
            message = result.Message
        });
    }
}
