using Casino.Backend.Enums;
using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for WalletTransaction operations
    /// </summary>
    public interface IWalletTransactionRepository : IRepository<WalletTransaction>
    {
      /// <summary>
      /// Get all transactions for a specific user
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Get transactions for a user filtered by type
        /// </summary>
        Task<IEnumerable<WalletTransaction>> GetByUserIdAndTypeAsync(int userId, WalletTransactionType type);

        /// <summary>
        /// Get transactions for a user within a date range
        /// </summary>
      Task<IEnumerable<WalletTransaction>> GetByUserIdAndDateRangeAsync(int userId, DateTime from, DateTime to);

     /// <summary>
        /// Get transaction by bet ID
        /// </summary>
      Task<IEnumerable<WalletTransaction>> GetByBetIdAsync(int betId);

    /// <summary>
   /// Get recent transactions for a user (last N transactions)
      /// </summary>
 Task<IEnumerable<WalletTransaction>> GetRecentTransactionsAsync(int userId, int count = 10);

        /// <summary>
     /// Get total deposited amount for a user
        /// </summary>
        Task<decimal> GetTotalDepositedAsync(int userId);

    /// <summary>
        /// Get total withdrawn amount for a user
        /// </summary>
        Task<decimal> GetTotalWithdrawnAsync(int userId);

        /// <summary>
        /// Get transaction statistics for a user
        /// </summary>
      Task<WalletTransactionStats> GetUserStatsAsync(int userId);
    }

    /// <summary>
    /// Statistics summary for user wallet transactions
    /// </summary>
    public class WalletTransactionStats
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
}
