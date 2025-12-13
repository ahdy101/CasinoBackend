namespace Casino.Backend.Services.Interfaces
{
/// <summary>
/// Service for managing user wallet operations with atomic transaction support
    /// </summary>
public interface IWalletService
    {
     /// <summary>
        /// Deduct bet amount from user's wallet atomically
        /// </summary>
  /// <param name="userId">User ID</param>
        /// <param name="amount">Amount to deduct</param>
        /// <returns>Transaction result with bet ID if successful</returns>
    Task<WalletTransactionResult> DeductBet(int userId, decimal amount, string gameType, string choice);

        /// <summary>
        /// Process payout to user's wallet atomically
      /// </summary>
      /// <param name="userId">User ID</param>
    /// <param name="betId">Original bet ID</param>
        /// <param name="payoutAmount">Amount to credit</param>
        /// <returns>Transaction result</returns>
        Task<WalletTransactionResult> ProcessPayout(int userId, int betId, decimal payoutAmount);

   /// <summary>
   /// Get current balance for a user
 /// </summary>
        /// <param name="userId">User ID</param>
   /// <returns>Current balance</returns>
        Task<decimal> GetBalance(int userId);

     /// <summary>
    /// Add funds to user's wallet (for admin purposes)
        /// </summary>
 /// <param name="userId">User ID</param>
 /// <param name="amount">Amount to add</param>
      /// <returns>Transaction result</returns>
      Task<WalletTransactionResult> AddFunds(int userId, decimal amount);
    }

    /// <summary>
    /// Result of a wallet transaction operation
    /// </summary>
    public class WalletTransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
     public int? BetId { get; set; }
     public decimal NewBalance { get; set; }
        public List<string> Errors { get; set; } = new();

        public static WalletTransactionResult Successful(int? betId, decimal newBalance, string message = "Transaction successful")
        {
            return new WalletTransactionResult
            {
      Success = true,
 Message = message,
                BetId = betId,
    NewBalance = newBalance
   };
  }

        public static WalletTransactionResult UserNotFound()
        {
     return new WalletTransactionResult
 {
    Success = false,
            Message = "User not found",
       Errors = new List<string> { "The specified user does not exist." }
            };
        }

        public static WalletTransactionResult InsufficientFunds(decimal required, decimal available)
        {
            return new WalletTransactionResult
          {
     Success = false,
       Message = "Insufficient funds",
     Errors = new List<string> { $"Required: {required:C}, Available: {available:C}" }
            };
        }

        public static WalletTransactionResult Error(string errorMessage)
  {
            return new WalletTransactionResult
   {
                Success = false,
        Message = "Transaction failed",
Errors = new List<string> { errorMessage }
  };
        }
    }
}
