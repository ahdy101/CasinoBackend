using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Casino.Backend.Services.Implementations
{
    /// <summary>
    /// Wallet service implementation with atomic database transactions
    /// ?? CRITICAL: All operations use transactions to ensure data consistency
    /// </summary>
    public class WalletService : IWalletService
  {
    private readonly AppDbContext _db;
      private readonly ILogger<WalletService> _logger;

      public WalletService(AppDbContext db, ILogger<WalletService> logger)
        {
  _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Deduct bet amount from user's wallet atomically
      /// </summary>
        public async Task<WalletTransactionResult> DeductBet(int userId, decimal amount, string gameType, string choice)
 {
  using var transaction = await _db.Database.BeginTransactionAsync();
        try
            {
             _logger.LogInformation("DeductBet - UserId: {UserId}, Amount: {Amount}, Game: {Game}", userId, amount, gameType);

              // Validate amount
       if (amount <= 0)
   {
          return WalletTransactionResult.Error("Bet amount must be greater than zero");
        }

     // Get user with row lock
 var user = await _db.Users.FindAsync(userId);
    if (user == null)
    {
            _logger.LogWarning("DeductBet - User not found: {UserId}", userId);
return WalletTransactionResult.UserNotFound();
           }

    // Check balance
  if (user.Balance < amount)
   {
    _logger.LogWarning("DeductBet - Insufficient funds. UserId: {UserId}, Required: {Required}, Available: {Available}", 
     userId, amount, user.Balance);
   return WalletTransactionResult.InsufficientFunds(amount, user.Balance);
    }

// Deduct from balance
     user.Balance -= amount;

       // Create bet record
      var bet = new Bet
          {
             UserId = userId,
      Game = gameType,
         Amount = amount,
    Choice = choice,
    Payout = 0m, // Will be updated when game completes
             CreatedAt = DateTime.UtcNow
   };

           _db.Bets.Add(bet);
     await _db.SaveChangesAsync();
   await transaction.CommitAsync();

       _logger.LogInformation("DeductBet successful - BetId: {BetId}, NewBalance: {Balance}", bet.Id, user.Balance);

     return WalletTransactionResult.Successful(bet.Id, user.Balance, "Bet placed successfully");
     }
      catch (Exception ex)
   {
       await transaction.RollbackAsync();
   _logger.LogError(ex, "DeductBet failed - UserId: {UserId}, Amount: {Amount}", userId, amount);
              return WalletTransactionResult.Error($"Transaction failed: {ex.Message}");
     }
        }

        /// <summary>
        /// Process payout to user's wallet atomically
        /// </summary>
  public async Task<WalletTransactionResult> ProcessPayout(int userId, int betId, decimal payoutAmount)
        {
       using var transaction = await _db.Database.BeginTransactionAsync();
try
 {
            _logger.LogInformation("ProcessPayout - UserId: {UserId}, BetId: {BetId}, Payout: {Payout}", userId, betId, payoutAmount);

         // Get user
    var user = await _db.Users.FindAsync(userId);
if (user == null)
      {
                 _logger.LogWarning("ProcessPayout - User not found: {UserId}", userId);
      return WalletTransactionResult.UserNotFound();
              }

         // Get bet
          var bet = await _db.Bets.FindAsync(betId);
            if (bet == null)
    {
   _logger.LogWarning("ProcessPayout - Bet not found: {BetId}", betId);
 return WalletTransactionResult.Error("Bet not found");
              }

         // Verify bet belongs to user
     if (bet.UserId != userId)
             {
_logger.LogWarning("ProcessPayout - Bet does not belong to user. BetId: {BetId}, UserId: {UserId}", betId, userId);
 return WalletTransactionResult.Error("Bet does not belong to user");
      }

   // Add payout to balance
       user.Balance += payoutAmount;

         // Update bet payout
         bet.Payout = payoutAmount;

          await _db.SaveChangesAsync();
          await transaction.CommitAsync();

 _logger.LogInformation("ProcessPayout successful - BetId: {BetId}, Payout: {Payout}, NewBalance: {Balance}", 
       betId, payoutAmount, user.Balance);

      return WalletTransactionResult.Successful(betId, user.Balance, "Payout processed successfully");
          }
      catch (Exception ex)
        {
         await transaction.RollbackAsync();
   _logger.LogError(ex, "ProcessPayout failed - UserId: {UserId}, BetId: {BetId}", userId, betId);
     return WalletTransactionResult.Error($"Transaction failed: {ex.Message}");
   }
        }

        /// <summary>
        /// Get current balance for a user
        /// </summary>
        public async Task<decimal> GetBalance(int userId)
   {
 var user = await _db.Users
       .AsNoTracking()
         .FirstOrDefaultAsync(u => u.Id == userId);

 return user?.Balance ?? 0m;
        }

        /// <summary>
   /// Add funds to user's wallet (for admin purposes or bonuses)
        /// </summary>
        public async Task<WalletTransactionResult> AddFunds(int userId, decimal amount)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
    try
   {
      _logger.LogInformation("AddFunds - UserId: {UserId}, Amount: {Amount}", userId, amount);

         if (amount <= 0)
       {
   return WalletTransactionResult.Error("Amount must be greater than zero");
        }

    var user = await _db.Users.FindAsync(userId);
              if (user == null)
       {
        _logger.LogWarning("AddFunds - User not found: {UserId}", userId);
       return WalletTransactionResult.UserNotFound();
 }

     user.Balance += amount;
       await _db.SaveChangesAsync();
                await transaction.CommitAsync();

          _logger.LogInformation("AddFunds successful - UserId: {UserId}, Amount: {Amount}, NewBalance: {Balance}", 
    userId, amount, user.Balance);

    return WalletTransactionResult.Successful(null, user.Balance, "Funds added successfully");
   }
            catch (Exception ex)
      {
  await transaction.RollbackAsync();
 _logger.LogError(ex, "AddFunds failed - UserId: {UserId}, Amount: {Amount}", userId, amount);
     return WalletTransactionResult.Error($"Transaction failed: {ex.Message}");
        }
        }
    }
}
