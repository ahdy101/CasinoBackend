using Casino_Api.DTOs.Responses;
using Casino_Api.Security;
using Casino_Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Casino_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly TokenValidator _tokenValidator;

    public WalletController(
        IWalletService walletService,
    TokenValidator tokenValidator)
    {
        _walletService = walletService;
      _tokenValidator = tokenValidator;
    }

    /// <summary>
    /// Get user balance (Requires: Bearer token only)
    /// </summary>
    [HttpGet("balance")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBalance(
        [FromHeader(Name = "Authorization")] string? authorization)
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

     var result = await _walletService.GetBalance(validation.UserId);

        if (!result.Success)
 return NotFound(new ErrorResponse("User not found"));

    return Ok(new { balance = result.Balance });
    }

    /// <summary>
    /// Add funds to wallet (Requires: Bearer token only)
    /// </summary>
    [HttpPost("add-funds")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddFunds(
        [FromHeader(Name = "Authorization")] string? authorization,
        [FromQuery] decimal amount)
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

        var result = await _walletService.AddFunds(validation.UserId, amount);

        if (!result.Success)
   return BadRequest(new ErrorResponse(result.Message));

   return Ok(new
        {
      success = true,
          newBalance = result.NewBalance,
  message = result.Message
    });
    }

    /// <summary>
    /// Cash out from wallet (Requires: Bearer token only)
    /// </summary>
    [HttpPost("cash-out")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CashOut(
        [FromHeader(Name = "Authorization")] string? authorization,
        [FromQuery] decimal amount)
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

var result = await _walletService.CashOut(validation.UserId, amount);

    if (!result.Success)
    return BadRequest(new ErrorResponse(result.Message));

        return Ok(new
        {
  success = true,
       newBalance = result.NewBalance,
 message = result.Message
        });
    }
}
