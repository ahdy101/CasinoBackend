using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;

namespace Casino.Backend.Repositories.Implementations
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
 private readonly IDbConnectionFactory _connectionFactory;

        public PasswordResetTokenRepository(IDbConnectionFactory connectionFactory)
        {
        _connectionFactory = connectionFactory;
    }

  public async Task<PasswordResetToken?> GetByIdAsync(int id)
        {
      const string sql = @"
        SELECT Id, UserId, Token, ExpiresAt, IsUsed, CreatedAt, UsedAt
   FROM PasswordResetTokens
    WHERE Id = @Id";

    using var connection = _connectionFactory.CreateConnection();
      return await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(sql, new { Id = id });
      }

  public async Task<IEnumerable<PasswordResetToken>> GetAllAsync()
        {
   const string sql = @"
      SELECT Id, UserId, Token, ExpiresAt, IsUsed, CreatedAt, UsedAt
FROM PasswordResetTokens
    ORDER BY CreatedAt DESC";

      using var connection = _connectionFactory.CreateConnection();
   return await connection.QueryAsync<PasswordResetToken>(sql);
        }

 public async Task<int> AddAsync(PasswordResetToken entity)
        {
            const string sql = @"
   INSERT INTO PasswordResetTokens (UserId, Token, ExpiresAt, IsUsed, CreatedAt)
    VALUES (@UserId, @Token, @ExpiresAt, @IsUsed, @CreatedAt);
  SELECT LAST_INSERT_ID();";

using var connection = _connectionFactory.CreateConnection();
var id = await connection.ExecuteScalarAsync<int>(sql, entity);
       entity.Id = id;
      return id;
   }

    public async Task<bool> UpdateAsync(PasswordResetToken entity)
  {
    const string sql = @"
         UPDATE PasswordResetTokens
    SET IsUsed = @IsUsed, UsedAt = @UsedAt
   WHERE Id = @Id";

  using var connection = _connectionFactory.CreateConnection();
       var rowsAffected = await connection.ExecuteAsync(sql, entity);
      return rowsAffected > 0;
        }

     public async Task<bool> DeleteAsync(int id)
   {
      const string sql = "DELETE FROM PasswordResetTokens WHERE Id = @Id";

         using var connection = _connectionFactory.CreateConnection();
      var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
return rowsAffected > 0;
        }

 public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
  const string sql = @"
     SELECT Id, UserId, Token, ExpiresAt, IsUsed, CreatedAt, UsedAt
       FROM PasswordResetTokens
       WHERE Token = @Token
 LIMIT 1";

  using var connection = _connectionFactory.CreateConnection();
  return await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(sql, new { Token = token });
   }

public async Task<PasswordResetToken?> GetActiveTokenByUserIdAsync(int userId)
   {
const string sql = @"
      SELECT Id, UserId, Token, ExpiresAt, IsUsed, CreatedAt, UsedAt
    FROM PasswordResetTokens
       WHERE UserId = @UserId 
       AND IsUsed = 0 
           AND ExpiresAt > @Now
     ORDER BY CreatedAt DESC
       LIMIT 1";

  using var connection = _connectionFactory.CreateConnection();
       return await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(sql, 
        new { UserId = userId, Now = DateTime.UtcNow });
        }

  public async Task InvalidateUserTokensAsync(int userId)
  {
      const string sql = @"
      UPDATE PasswordResetTokens
         SET IsUsed = 1
             WHERE UserId = @UserId AND IsUsed = 0";

 using var connection = _connectionFactory.CreateConnection();
    await connection.ExecuteAsync(sql, new { UserId = userId });
        }
    }
}
