using Casino.Backend.Data;
using Casino.Backend.Enums;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;

namespace Casino.Backend.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
     private readonly IDbConnectionFactory _connectionFactory;

        public PaymentRepository(IDbConnectionFactory connectionFactory)
   {
 _connectionFactory = connectionFactory;
   }

        public async Task<Payment?> GetByIdAsync(int id)
    {
 using var connection = _connectionFactory.CreateConnection();
   const string sql = "SELECT * FROM Payments WHERE Id = @Id";
return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { Id = id });
        }

  public async Task<IEnumerable<Payment>> GetAllAsync()
        {
     using var connection = _connectionFactory.CreateConnection();
          const string sql = "SELECT * FROM Payments ORDER BY CreatedAt DESC";
 return await connection.QueryAsync<Payment>(sql);
      }

   public async Task<int> AddAsync(Payment entity)
        {
      using var connection = _connectionFactory.CreateConnection();
     const string sql = @"
    INSERT INTO Payments 
       (UserId, Type, Amount, Provider, Method, Status, ExternalTransactionId, 
       ReferenceNumber, WalletTransactionId, Currency, Fee, NetAmount, 
    IpAddress, Metadata, ErrorMessage, CreatedAt, CompletedAt, UpdatedAt)
 VALUES 
  (@UserId, @Type, @Amount, @Provider, @Method, @Status, @ExternalTransactionId,
 @ReferenceNumber, @WalletTransactionId, @Currency, @Fee, @NetAmount,
    @IpAddress, @Metadata, @ErrorMessage, @CreatedAt, @CompletedAt, @UpdatedAt);
  SELECT LAST_INSERT_ID();";

entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
return entity.Id;
        }

  public async Task<bool> UpdateAsync(Payment entity)
        {
       using var connection = _connectionFactory.CreateConnection();
 const string sql = @"
           UPDATE Payments 
SET Status = @Status,
   ExternalTransactionId = @ExternalTransactionId,
    WalletTransactionId = @WalletTransactionId,
ErrorMessage = @ErrorMessage,
   CompletedAt = @CompletedAt,
   UpdatedAt = @UpdatedAt
     WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, entity);
      return rowsAffected > 0;
  }

      public async Task<bool> DeleteAsync(int id)
        {
    using var connection = _connectionFactory.CreateConnection();
     const string sql = "DELETE FROM Payments WHERE Id = @Id";
     var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
         return rowsAffected > 0;
      }

   public async Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId)
 {
     using var connection = _connectionFactory.CreateConnection();
   const string sql = "SELECT * FROM Payments WHERE ExternalTransactionId = @ExternalTransactionId";
            return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { ExternalTransactionId = externalTransactionId });
        }

   public async Task<Payment?> GetByReferenceNumberAsync(string referenceNumber)
{
   using var connection = _connectionFactory.CreateConnection();
         const string sql = "SELECT * FROM Payments WHERE ReferenceNumber = @ReferenceNumber";
     return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { ReferenceNumber = referenceNumber });
        }

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(int userId)
        {
   using var connection = _connectionFactory.CreateConnection();
       const string sql = "SELECT * FROM Payments WHERE UserId = @UserId ORDER BY CreatedAt DESC";
   return await connection.QueryAsync<Payment>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAndTypeAsync(int userId, PaymentType type)
{
     using var connection = _connectionFactory.CreateConnection();
   const string sql = "SELECT * FROM Payments WHERE UserId = @UserId AND Type = @Type ORDER BY CreatedAt DESC";
   return await connection.QueryAsync<Payment>(sql, new { UserId = userId, Type = type });
        }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
        {
  using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Payments WHERE Status = @Status ORDER BY CreatedAt DESC";
 return await connection.QueryAsync<Payment>(sql, new { Status = status });
        }

        public async Task<IEnumerable<Payment>> GetPendingWithdrawalsAsync()
{
       using var connection = _connectionFactory.CreateConnection();
   const string sql = @"
 SELECT * FROM Payments 
 WHERE Type = @WithdrawalType 
   AND Status = @PendingStatus 
       ORDER BY CreatedAt ASC";

return await connection.QueryAsync<Payment>(sql, new 
 { 
     WithdrawalType = PaymentType.Withdrawal,
   PendingStatus = PaymentStatus.Pending 
      });
  }

      public async Task<bool> UpdateStatusAsync(int paymentId, PaymentStatus status, string? errorMessage = null)
{
    using var connection = _connectionFactory.CreateConnection();
    const string sql = @"
       UPDATE Payments 
         SET Status = @Status,
    ErrorMessage = @ErrorMessage,
CompletedAt = CASE WHEN @Status IN (3, 4, 5, 6) THEN NOW() ELSE CompletedAt END,
  UpdatedAt = NOW()
  WHERE Id = @PaymentId";

    var rowsAffected = await connection.ExecuteAsync(sql, new 
   { 
     PaymentId = paymentId,
    Status = status,
ErrorMessage = errorMessage
  });

     return rowsAffected > 0;
        }
    }
}
