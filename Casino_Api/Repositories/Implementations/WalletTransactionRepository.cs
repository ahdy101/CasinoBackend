using Casino.Backend.Data;
using Casino.Backend.Enums;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;
using System.Data;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
    /// Dapper implementation of WalletTransaction repository
    /// </summary>
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public WalletTransactionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<WalletTransaction?> GetByIdAsync(int id)
        {
          using var connection = _connectionFactory.CreateConnection();
     const string sql = @"
                SELECT * FROM WalletTransactions 
         WHERE Id = @Id";
        
    return await connection.QueryFirstOrDefaultAsync<WalletTransaction>(sql, new { Id = id });
        }

        public async Task<IEnumerable<WalletTransaction>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
   const string sql = "SELECT * FROM WalletTransactions ORDER BY CreatedAt DESC";
        
         return await connection.QueryAsync<WalletTransaction>(sql);
        }

        public async Task<int> AddAsync(WalletTransaction entity)
        {
using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
       INSERT INTO WalletTransactions 
      (UserId, Type, Amount, BalanceBefore, BalanceAfter, BetId, GameType, 
      Description, Status, ReferenceNumber, IpAddress, CreatedAt, ProcessedAt)
        VALUES 
     (@UserId, @Type, @Amount, @BalanceBefore, @BalanceAfter, @BetId, @GameType,
    @Description, @Status, @ReferenceNumber, @IpAddress, @CreatedAt, @ProcessedAt);
        SELECT LAST_INSERT_ID();";

            entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
            return entity.Id;
        }

      public async Task<bool> UpdateAsync(WalletTransaction entity)
     {
            using var connection = _connectionFactory.CreateConnection();
   const string sql = @"
   UPDATE WalletTransactions 
                SET Status = @Status,
      ProcessedAt = @ProcessedAt,
           Description = @Description
   WHERE Id = @Id";
 
    var rowsAffected = await connection.ExecuteAsync(sql, entity);
          return rowsAffected > 0;
 }

      public async Task<bool> DeleteAsync(int id)
        {
   using var connection = _connectionFactory.CreateConnection();
          const string sql = "DELETE FROM WalletTransactions WHERE Id = @Id";
            
  var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
       return rowsAffected > 0;
        }

        public async Task<IEnumerable<WalletTransaction>> GetByUserIdAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
         SELECT * FROM WalletTransactions 
WHERE UserId = @UserId 
 ORDER BY CreatedAt DESC";
   
 return await connection.QueryAsync<WalletTransaction>(sql, new { UserId = userId });
 }

        public async Task<IEnumerable<WalletTransaction>> GetByUserIdAndTypeAsync(int userId, WalletTransactionType type)
        {
  using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM WalletTransactions 
      WHERE UserId = @UserId AND Type = @Type 
         ORDER BY CreatedAt DESC";
  
       return await connection.QueryAsync<WalletTransaction>(sql, new { UserId = userId, Type = type });
        }

        public async Task<IEnumerable<WalletTransaction>> GetByUserIdAndDateRangeAsync(int userId, DateTime from, DateTime to)
 {
  using var connection = _connectionFactory.CreateConnection();
  const string sql = @"
            SELECT * FROM WalletTransactions 
              WHERE UserId = @UserId 
            AND CreatedAt >= @From 
  AND CreatedAt <= @To 
    ORDER BY CreatedAt DESC";
  
     return await connection.QueryAsync<WalletTransaction>(sql, new { UserId = userId, From = from, To = to });
        }

  public async Task<IEnumerable<WalletTransaction>> GetByBetIdAsync(int betId)
        {
       using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
       SELECT * FROM WalletTransactions 
  WHERE BetId = @BetId 
      ORDER BY CreatedAt";
 
  return await connection.QueryAsync<WalletTransaction>(sql, new { BetId = betId });
   }

        public async Task<IEnumerable<WalletTransaction>> GetRecentTransactionsAsync(int userId, int count = 10)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
  SELECT * FROM WalletTransactions 
   WHERE UserId = @UserId 
   ORDER BY CreatedAt DESC 
   LIMIT @Count";
            
       return await connection.QueryAsync<WalletTransaction>(sql, new { UserId = userId, Count = count });
 }

 public async Task<decimal> GetTotalDepositedAsync(int userId)
        {
         using var connection = _connectionFactory.CreateConnection();
     const string sql = @"
      SELECT COALESCE(SUM(Amount), 0) 
              FROM WalletTransactions 
     WHERE UserId = @UserId 
         AND Type = @DepositType 
          AND Status = @CompletedStatus";
       
     return await connection.ExecuteScalarAsync<decimal>(sql, new 
        { 
                UserId = userId, 
                DepositType = WalletTransactionType.Deposit,
     CompletedStatus = WalletTransactionStatus.Completed
         });
        }

    public async Task<decimal> GetTotalWithdrawnAsync(int userId)
        {
       using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
   SELECT COALESCE(SUM(Amount), 0) 
                FROM WalletTransactions 
      WHERE UserId = @UserId 
   AND Type = @WithdrawalType 
           AND Status = @CompletedStatus";
     
    return await connection.ExecuteScalarAsync<decimal>(sql, new 
     { 
    UserId = userId, 
  WithdrawalType = WalletTransactionType.Withdrawal,
    CompletedStatus = WalletTransactionStatus.Completed
        });
   }

  public async Task<WalletTransactionStats> GetUserStatsAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
 SELECT 
   COUNT(*) as TotalTransactions,
            COALESCE(SUM(CASE WHEN Type = @DepositType THEN Amount ELSE 0 END), 0) as TotalDeposited,
        COALESCE(SUM(CASE WHEN Type = @WithdrawalType THEN ABS(Amount) ELSE 0 END), 0) as TotalWithdrawn,
         COALESCE(SUM(CASE WHEN Type = @BetType THEN ABS(Amount) ELSE 0 END), 0) as TotalBets,
         COALESCE(SUM(CASE WHEN Type = @PayoutType THEN Amount ELSE 0 END), 0) as TotalPayouts,
      COALESCE(SUM(CASE WHEN Type = @BonusType THEN Amount ELSE 0 END), 0) as TotalBonuses,
        MIN(CreatedAt) as FirstTransactionDate,
           MAX(CreatedAt) as LastTransactionDate
         FROM WalletTransactions 
      WHERE UserId = @UserId 
       AND Status = @CompletedStatus";
        
            var stats = await connection.QueryFirstOrDefaultAsync<WalletTransactionStats>(sql, new 
      { 
   UserId = userId,
        DepositType = WalletTransactionType.Deposit,
                WithdrawalType = WalletTransactionType.Withdrawal,
        BetType = WalletTransactionType.Bet,
      PayoutType = WalletTransactionType.Payout,
                BonusType = WalletTransactionType.Bonus,
            CompletedStatus = WalletTransactionStatus.Completed
   });

            if (stats != null)
  {
                stats.NetProfit = stats.TotalPayouts - stats.TotalBets + stats.TotalBonuses;
   }

       return stats ?? new WalletTransactionStats();
        }
    }
}
