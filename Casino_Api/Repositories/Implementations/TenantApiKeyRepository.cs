using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
    /// Dapper-based repository for TenantApiKey using raw SQL
    /// </summary>
    public class TenantApiKeyRepository : ITenantApiKeyRepository
    {
  private readonly IDbConnectionFactory _connectionFactory;

   public TenantApiKeyRepository(IDbConnectionFactory connectionFactory)
      {
_connectionFactory = connectionFactory;
        }

        public async Task<TenantApiKey?> GetByIdAsync(int id)
  {
      const string sql = @"
 SELECT Id, TenantName, ApiKey, IsActive, TenantId, CreatedAt
     FROM TenantApiKeys
   WHERE Id = @Id";

    using var connection = _connectionFactory.CreateConnection();
      return await connection.QueryFirstOrDefaultAsync<TenantApiKey>(sql, new { Id = id });
 }

 public async Task<IEnumerable<TenantApiKey>> GetAllAsync()
        {
    const string sql = @"
SELECT Id, TenantName, ApiKey, IsActive, TenantId, CreatedAt
    FROM TenantApiKeys
 ORDER BY CreatedAt DESC";

      using var connection = _connectionFactory.CreateConnection();
 return await connection.QueryAsync<TenantApiKey>(sql);
    }

        public async Task<int> AddAsync(TenantApiKey entity)
    {
   const string sql = @"
 INSERT INTO TenantApiKeys (TenantName, ApiKey, IsActive, TenantId, CreatedAt)
  VALUES (@TenantName, @ApiKey, @IsActive, @TenantId, @CreatedAt);
    SELECT LAST_INSERT_ID();";

using var connection = _connectionFactory.CreateConnection();
  var id = await connection.ExecuteScalarAsync<int>(sql, entity);
         entity.Id = id;
      return id;
 }

  public async Task<bool> UpdateAsync(TenantApiKey entity)
   {
   const string sql = @"
   UPDATE TenantApiKeys
     SET TenantName = @TenantName,
   ApiKey = @ApiKey,
       IsActive = @IsActive,
       TenantId = @TenantId
   WHERE Id = @Id";

    using var connection = _connectionFactory.CreateConnection();
    var rowsAffected = await connection.ExecuteAsync(sql, entity);
     return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
      const string sql = "DELETE FROM TenantApiKeys WHERE Id = @Id";

   using var connection = _connectionFactory.CreateConnection();
  var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
return rowsAffected > 0;
        }

     public async Task<TenantApiKey?> GetByApiKeyAsync(string apiKey)
   {
const string sql = @"
 SELECT Id, TenantName, ApiKey, IsActive, TenantId, CreatedAt
  FROM TenantApiKeys
    WHERE ApiKey = @ApiKey
 LIMIT 1";

      using var connection = _connectionFactory.CreateConnection();
    return await connection.QueryFirstOrDefaultAsync<TenantApiKey>(sql, new { ApiKey = apiKey });
        }

 public async Task<bool> ValidateApiKeyAsync(string apiKey)
{
const string sql = "SELECT COUNT(1) FROM TenantApiKeys WHERE ApiKey = @ApiKey AND IsActive = 1";

     using var connection = _connectionFactory.CreateConnection();
   var count = await connection.ExecuteScalarAsync<int>(sql, new { ApiKey = apiKey });
return count > 0;
   }
    }
}
