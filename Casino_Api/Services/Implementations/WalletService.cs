using Casino.Backend.Data;
using Casino.Backend.Enums;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Casino.Backend.Services.Interfaces;
using MySqlConnector;

namespace Casino.Backend.Services.Implementations
{
    /// <summary>
    /// Wallet service implementation with atomic database transactions
    /// ?? CRITICAL: All operations use transactions to ensure data consistency
    /// </summary>
    public class WalletService : IWalletService
 {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IUserRepository _userRepository;
        private readonly IBetRepository _betRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
private readonly ILogger<WalletService> _logger;

     public WalletService(
          IDbConnectionFactory connectionFactory,
 IUserRepository userRepository,
    IBetRepository betRepository,
         IWalletTransactionRepository walletTransactionRepository,
            ILogger<WalletService> logger)
  {
   _connectionFactory = connectionFactory;
            _userRepository = userRepository;
      _betRepository = betRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _logger = logger;
        }

        /// <summary>
        /// Deduct bet amount from user's wallet atomically
        /// </summary>
        public async Task<WalletTransactionResult> DeductBet(int userId, decimal amount, string gameType, string choice)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
          
try
  {
       _logger.LogInformation("DeductBet - UserId: {UserId}, Amount: {Amount}, Game: {Game}", userId, amount, gameType);

 // Validate amount
      if (amount <= 0)
            {
   return WalletTransactionResult.Error("Bet amount must be greater than zero");
    }

    // Get user
    var user = await _userRepository.GetByIdAsync(userId);
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

         var balanceBefore = user.Balance;

      // Deduct from balance
      user.Balance -= amount;
                await _userRepository.UpdateAsync(user);

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

    await _betRepository.AddAsync(bet);

          // Log wallet transaction
             var walletTransaction = new WalletTransaction
         {
   UserId = userId,
        Type = WalletTransactionType.Bet,
    Amount = -amount, // Negative for debit
  BalanceBefore = balanceBefore,
      BalanceAfter = user.Balance,
     BetId = bet.Id,
           GameType = gameType,
      Description = $"Bet placed on {gameType} - {choice}",
              Status = WalletTransactionStatus.Completed,
       CreatedAt = DateTime.UtcNow,
  ProcessedAt = DateTime.UtcNow
   };

  await _walletTransactionRepository.AddAsync(walletTransaction);
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
         using var connection = _connectionFactory.CreateConnection();
       await connection.OpenAsync();
       using var transaction = await connection.BeginTransactionAsync();
     
        try
  {
        _logger.LogInformation("ProcessPayout - UserId: {UserId}, BetId: {BetId}, Payout: {Payout}", userId, betId, payoutAmount);

          // Get user
  var user = await _userRepository.GetByIdAsync(userId);
          if (user == null)
         {
        _logger.LogWarning("ProcessPayout - User not found: {UserId}", userId);
     return WalletTransactionResult.UserNotFound();
  }

      // Get bet
              var bet = await _betRepository.GetByIdAsync(betId);
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

                var balanceBefore = user.Balance;

   // Add payout to balance
     user.Balance += payoutAmount;
                await _userRepository.UpdateAsync(user);

   // Update bet payout
await _betRepository.UpdatePayoutAsync(betId, payoutAmount);

             // Log wallet transaction
     var walletTransaction = new WalletTransaction
    {
       UserId = userId,
 Type = WalletTransactionType.Payout,
        Amount = payoutAmount, // Positive for credit
        BalanceBefore = balanceBefore,
   BalanceAfter = user.Balance,
        BetId = betId,
         GameType = bet.Game,
         Description = $"Payout from {bet.Game} - Won {payoutAmount:C}",
      Status = WalletTransactionStatus.Completed,
       CreatedAt = DateTime.UtcNow,
         ProcessedAt = DateTime.UtcNow
  };

      await _walletTransactionRepository.AddAsync(walletTransaction);
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
    return await _userRepository.GetBalanceAsync(userId);
        }

     /// <summary>
   /// Add funds to user's wallet (for admin purposes or bonuses)
        /// </summary>
public async Task<WalletTransactionResult> AddFunds(int userId, decimal amount)
     {
         using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
  using var transaction = await connection.BeginTransactionAsync();
         
            try
          {
    _logger.LogInformation("AddFunds - UserId: {UserId}, Amount: {Amount}", userId, amount);

       if (amount <= 0)
     {
     return WalletTransactionResult.Error("Amount must be greater than zero");
         }

              var user = await _userRepository.GetByIdAsync(userId);
if (user == null)
      {
            _logger.LogWarning("AddFunds - User not found: {UserId}", userId);
           return WalletTransactionResult.UserNotFound();
      }

       var balanceBefore = user.Balance;

                user.Balance += amount;
                await _userRepository.UpdateAsync(user);

           // Log wallet transaction
      var walletTransaction = new WalletTransaction
   {
         UserId = userId,
     Type = WalletTransactionType.Deposit,
     Amount = amount, // Positive for credit
      BalanceBefore = balanceBefore,
           BalanceAfter = user.Balance,
        Description = $"Funds added to wallet - {amount:C}",
    Status = WalletTransactionStatus.Completed,
         CreatedAt = DateTime.UtcNow,
          ProcessedAt = DateTime.UtcNow
    };

            await _walletTransactionRepository.AddAsync(walletTransaction);
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
