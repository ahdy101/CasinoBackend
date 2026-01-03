using Casino.Backend.DTOs.Responses;
using Casino.Backend.Enums;
using Casino.Backend.Models;
using Casino.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Casino.Backend.Controllers
{
    /// <summary>
 /// Payment processing controller for deposits and withdrawals
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
   {
     private readonly IPaymentService _paymentService;
  private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
 IPaymentService paymentService,
   ILogger<PaymentsController> logger)
  {
  _paymentService = paymentService;
         _logger = logger;
  }

   /// <summary>
/// Create a deposit payment intent
  /// </summary>
 [HttpPost("deposit")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
 [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
 public async Task<IActionResult> CreateDeposit(
  [FromHeader(Name = "Authorization")] string? authorization,
            [FromBody] CreateDepositRequest request)
        {
 var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
  var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

   if (request.Amount <= 0)
    {
     return BadRequest(new ErrorResponse { Message = "Amount must be greater than zero" });
       }

  if (request.Amount < 10)
   {
return BadRequest(new ErrorResponse { Message = "Minimum deposit is $10" });
 }

  if (request.Amount > 10000)
{
return BadRequest(new ErrorResponse { Message = "Maximum deposit is $10,000 per transaction" });
  }

    var result = await _paymentService.CreateDepositAsync(
     userId,
        request.Amount,
       request.Provider,
 request.Method,
ipAddress);

     if (!result.Success)
{
return BadRequest(new ErrorResponse 
     { 
       Message = result.Message,
    Errors = result.Errors 
        });
     }

 return Ok(new PaymentResponseDto
   {
        Success = true,
            PaymentId = result.PaymentId!.Value,
     Status = result.Status.ToString(),
   ClientSecret = result.ClientSecret,
   PaymentUrl = result.PaymentUrl,
Message = result.Message
  });
  }

        /// <summary>
        /// Webhook endpoint for payment providers (temporarily disabled)
        /// </summary>
      [HttpPost("webhook/{provider}")]
 [AllowAnonymous]
  public async Task<IActionResult> PaymentWebhook(string provider)
    {
      _logger.LogInformation("Received webhook from {Provider} (handler temporarily disabled)", provider);
      return Ok(new { received = true, message = "Webhook handler is being updated" });
        }

/// <summary>
/// Create a withdrawal request
   /// </summary>
[HttpPost("withdrawal")]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateWithdrawal(
   [FromHeader(Name = "Authorization")] string? authorization,
   [FromBody] CreateWithdrawalRequest request)
    {
var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

 if (request.Amount <= 0)
      {
return BadRequest(new ErrorResponse { Message = "Amount must be greater than zero" });
    }

   if (request.Amount < 20)
     {
     return BadRequest(new ErrorResponse { Message = "Minimum withdrawal is $20" });
    }

  var result = await _paymentService.CreateWithdrawalAsync(
   userId,
 request.Amount,
   request.Method,
  request.AccountDetails);

   if (!result.Success)
{
       return BadRequest(new ErrorResponse 
{
  Message = result.Message,
       Errors = result.Errors 
 });
   }

       return Ok(new PaymentResponseDto
    {
Success = true,
    PaymentId = result.PaymentId!.Value,
   Status = result.Status.ToString(),
   Message = result.Message
   });
}

    /// <summary>
        /// Get payment history
  /// </summary>
 [HttpGet("history")]
 [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentHistory(
[FromHeader(Name = "Authorization")] string? authorization,
 [FromQuery] PaymentType? type = null)
        {
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
     var payments = await _paymentService.GetUserPaymentsAsync(userId, type);

     var response = payments.Select(p => new PaymentDto
{
      Id = p.Id,
   Type = p.Type.ToString(),
      Amount = p.Amount,
   Provider = p.Provider.ToString(),
      Method = p.Method.ToString(),
    Status = p.Status.ToString(),
  ReferenceNumber = p.ReferenceNumber,
        Fee = p.Fee,
         NetAmount = p.NetAmount,
   CreatedAt = p.CreatedAt,
   CompletedAt = p.CompletedAt
   }).ToList();

    return Ok(response);
        }

  /// <summary>
        /// Get specific payment details
/// </summary>
        [HttpGet("{paymentId}")]
 [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
 [ProducesResponseType(StatusCodes.Status404NotFound)]
 public async Task<IActionResult> GetPayment(
 [FromHeader(Name = "Authorization")] string? authorization,
   int paymentId)
        {
  var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
   var payment = await _paymentService.GetPaymentByIdAsync(paymentId);

  if (payment == null || payment.UserId != userId)
      {
 return NotFound();
     }

         var response = new PaymentDto
{
Id = payment.Id,
Type = payment.Type.ToString(),
      Amount = payment.Amount,
      Provider = payment.Provider.ToString(),
   Method = payment.Method.ToString(),
     Status = payment.Status.ToString(),
 ReferenceNumber = payment.ReferenceNumber,
  Fee = payment.Fee,
     NetAmount = payment.NetAmount,
 CreatedAt = payment.CreatedAt,
 CompletedAt = payment.CompletedAt
 };

   return Ok(response);
        }

   /// <summary>
        /// Cancel a pending payment
 /// </summary>
  [HttpPost("{paymentId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      public async Task<IActionResult> CancelPayment(
   [FromHeader(Name = "Authorization")] string? authorization,
int paymentId)
      {
   var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _paymentService.CancelPaymentAsync(paymentId, userId);

    if (!result.Success)
   {
      return BadRequest(new ErrorResponse 
  { 
Message = result.Message,
  Errors = result.Errors 
     });
  }

     return Ok(new { message = result.Message });
  }
    }

    // DTOs
    public class CreateDepositRequest
    {
        public decimal Amount { get; set; }
        public PaymentProvider Provider { get; set; } = PaymentProvider.Stripe;
   public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard;
    }

    public class CreateWithdrawalRequest
 {
   public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
  public string? AccountDetails { get; set; }  // Bank account, PayPal email, crypto address, etc.
    }

    public class PaymentResponseDto
    {
     public bool Success { get; set; }
   public int PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ClientSecret { get; set; }
        public string? PaymentUrl { get; set; }
        public string Message { get; set; } = string.Empty;
    }

 public class PaymentDto
    {
        public int Id { get; set; }
  public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Provider { get; set; } = string.Empty;
 public string Method { get; set; } = string.Empty;
 public string Status { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
