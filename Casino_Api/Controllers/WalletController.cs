using Casino.Backend.DTOs.Responses;
using Casino.Backend.Enums;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Casino.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Casino.Backend.Controllers
{
    /// <summary>
    /// Wallet management and transaction history controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
   private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly ILogger<WalletController> _logger;

        public WalletController(
            IWalletService walletService,
         IWalletTransactionRepository walletTransactionRepository,
            ILogger<WalletController> logger)
        {
        _walletService = walletService;
    _walletTransactionRepository = walletTransactionRepository;
       _logger = logger;
  }

        /// <summary>
        /// Get current wallet balance
        /// </summary>
     [HttpGet("balance")]
        [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
     public async Task<IActionResult> GetBalance(
      [FromHeader(Name = "Authorization")][System.ComponentModel.DataAnnotations.Required] string authorization)
        {
       var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
      var balance = await _walletService.GetBalance(userId);

return Ok(new BalanceResponse
{
  UserId = userId,
    Balance = balance,
          Timestamp = DateTime.UtcNow
  });
}

   /// <summary>
        /// Get transaction history (paginated)
        /// </summary>
      [HttpGet("transactions")]
        [ProducesResponseType(typeof(TransactionHistoryResponse), StatusCodes.Status200OK)]
      public async Task<IActionResult> GetTransactions(
            [FromHeader(Name = "Authorization")][System.ComponentModel.DataAnnotations.Required] string authorization,
  [FromQuery] int page = 1,
  [FromQuery] int pageSize = 20,
      [FromQuery] WalletTransactionType? type = null)
        {
     var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

      IEnumerable<WalletTransaction> transactions;
   
  if (type.HasValue)
 {
transactions = await _walletTransactionRepository.GetByUserIdAndTypeAsync(userId, type.Value);
     }
  else
      {
         transactions = await _walletTransactionRepository.GetByUserIdAsync(userId);
    }

       // Paginate
     var paginatedTransactions = transactions
     .Skip((page - 1) * pageSize)
    .Take(pageSize)
     .Select(t => new TransactionDto
    {
       Id = t.Id,
     Type = t.Type.ToString(),
 Amount = t.Amount,
   BalanceBefore = t.BalanceBefore,
       BalanceAfter = t.BalanceAfter,
     Description = t.Description,
       GameType = t.GameType,
     Status = t.Status.ToString(),
           CreatedAt = t.CreatedAt
     })
   .ToList();

    return Ok(new TransactionHistoryResponse
    {
      Transactions = paginatedTransactions,
Page = page,
     PageSize = pageSize,
      TotalCount = transactions.Count()
      });
        }

   /// <summary>
        /// Get recent transactions (last 10)
    /// </summary>
   [HttpGet("transactions/recent")]
   [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentTransactions(
   [FromHeader(Name = "Authorization")][System.ComponentModel.DataAnnotations.Required] string authorization,
   [FromQuery] int count = 10)
  {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var transactions = await _walletTransactionRepository.GetRecentTransactionsAsync(userId, count);

     var response = transactions.Select(t => new TransactionDto
 {
     Id = t.Id,
        Type = t.Type.ToString(),
            Amount = t.Amount,
          BalanceBefore = t.BalanceBefore,
      BalanceAfter = t.BalanceAfter,
    Description = t.Description,
     GameType = t.GameType,
         Status = t.Status.ToString(),
      CreatedAt = t.CreatedAt
    }).ToList();

   return Ok(response);
  }

  /// <summary>
 /// Get transaction statistics
/// </summary>
  [HttpGet("statistics")]
  [ProducesResponseType(typeof(WalletStatsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics(
            [FromHeader(Name = "Authorization")][System.ComponentModel.DataAnnotations.Required] string authorization)
  {
       var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var stats = await _walletTransactionRepository.GetUserStatsAsync(userId);

     return Ok(new WalletStatsResponse
  {
       TotalTransactions = stats.TotalTransactions,
  TotalDeposited = stats.TotalDeposited,
 TotalWithdrawn = stats.TotalWithdrawn,
   TotalBets = stats.TotalBets,
       TotalPayouts = stats.TotalPayouts,
          TotalBonuses = stats.TotalBonuses,
        NetProfit = stats.NetProfit,
  FirstTransactionDate = stats.FirstTransactionDate,
            LastTransactionDate = stats.LastTransactionDate
   });
      }

  /// <summary>
 /// Add funds to wallet (for testing/demo purposes - would normally integrate with payment gateway)
   /// </summary>
[HttpPost("add-funds")]
  [ProducesResponseType(typeof(WalletOperationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddFunds(
  [FromHeader(Name = "Authorization")][System.ComponentModel.DataAnnotations.Required] string authorization,
          [FromBody] AddFundsRequest request)
  {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

       if (request.Amount <= 0)
     {
   return BadRequest(new ErrorResponse { Message = "Amount must be greater than zero" });
   }

            var result = await _walletService.AddFunds(userId, request.Amount);

   if (!result.Success)
     {
     return BadRequest(new ErrorResponse 
        { 
     Message = result.Message,
     Errors = result.Errors 
    });
    }

   return Ok(new WalletOperationResponse
 {
       Success = true,
   Message = result.Message,
   NewBalance = result.NewBalance
     });
        }

        /// <summary>
    /// Get transactions by date range
        /// </summary>
        [HttpGet("transactions/by-date")]
   [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
   public async Task<IActionResult> GetTransactionsByDateRange(
   [FromHeader(Name = "Authorization")][System.ComponentModel.DataAnnotations.Required] string authorization,
     [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var transactions = await _walletTransactionRepository.GetByUserIdAndDateRangeAsync(userId, from, to);

     var response = transactions.Select(t => new TransactionDto
         {
         Id = t.Id,
      Type = t.Type.ToString(),
      Amount = t.Amount,
BalanceBefore = t.BalanceBefore,
    BalanceAfter = t.BalanceAfter,
      Description = t.Description,
GameType = t.GameType,
        Status = t.Status.ToString(),
     CreatedAt = t.CreatedAt
 }).ToList();

       return Ok(response);
        }
    }

    // DTOs
    public class BalanceResponse
    {
   public int UserId { get; set; }
  public decimal Balance { get; set; }
    public DateTime Timestamp { get; set; }
  }

    public class TransactionDto
    {
      public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
     public decimal Amount { get; set; }
public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
   public string Description { get; set; } = string.Empty;
   public string? GameType { get; set; }
     public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    }

    public class TransactionHistoryResponse
    {
  public List<TransactionDto> Transactions { get; set; } = new();
        public int Page { get; set; }
     public int PageSize { get; set; }
        public int TotalCount { get; set; }
}

    public class WalletStatsResponse
    {
  public int TotalTransactions { get; set; }
     public decimal TotalDeposited { get; set; }
   public decimal TotalWithdrawn { get; set; }
public decimal TotalBets { get; set; }
  public decimal TotalPayouts { get; set; }
   public decimal TotalBonuses { get; set; }
   public decimal NetProfit { get; set; }
 public DateTime FirstTransactionDate { get; set; }
        public DateTime LastTransactionDate { get; set; }
    }

    public class WalletOperationResponse
    {
    public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
      public decimal NewBalance { get; set; }
    }

    public class AddFundsRequest
    {
     public decimal Amount { get; set; }
}
}
