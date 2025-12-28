using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;
using System.Data;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
    /// Dapper-based repository for TenantApiKey using raw SQL
    /// </summary>
    public class TenantApiKeyRepository : ITenantApiKeyRepository
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<TenantApiKeyRepository> _logger;

        public TenantApiKeyRepository(IDbConnectionFactory connectionFactory, ILogger<TenantApiKeyRepository> logger)
        {
            _connection = connectionFactory.CreateConnection();
            _logger = logger;
        }

        public async Task<TenantApiKey?> GetByIdAsync(int id)
        {
            try
            {
                const string sql = @"
 SELECT Id, TenantName, ApiKey, IsActive, TenantId, CreatedAt
     FROM TenantApiKeys
   WHERE Id = @Id";

                return await _connection.QueryFirstOrDefaultAsync<TenantApiKey>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant API key by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<TenantApiKey>> GetAllAsync()
        {
            try
            {
                const string sql = @"
SELECT Id, TenantName, ApiKey, IsActive, TenantId, CreatedAt
    FROM TenantApiKeys
 ORDER BY CreatedAt DESC";

                return await _connection.QueryAsync<TenantApiKey>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tenant API keys");
                throw;
            }
        }

        public async Task<int> AddAsync(TenantApiKey entity)
        {
            try
            {
                const string sql = @"
 INSERT INTO TenantApiKeys (TenantName, ApiKey, IsActive, TenantId, CreatedAt)
  VALUES (@TenantName, @ApiKey, @IsActive, @TenantId, @CreatedAt);
    SELECT LAST_INSERT_ID();";

                var id = await _connection.ExecuteScalarAsync<int>(sql, entity);
                entity.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tenant API key: {TenantName}", entity.TenantName);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(TenantApiKey entity)
        {
            try
            {
                const string sql = @"
   UPDATE TenantApiKeys
     SET TenantName = @TenantName,
   ApiKey = @ApiKey,
       IsActive = @IsActive,
       TenantId = @TenantId
   WHERE Id = @Id";

                var rowsAffected = await _connection.ExecuteAsync(sql, entity);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant API key: {Id}", entity.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                const string sql = "DELETE FROM TenantApiKeys WHERE Id = @Id";
                var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tenant API key: {Id}", id);
                throw;
            }
        }

        public async Task<TenantApiKey?> GetByApiKeyAsync(string apiKey)
        {
            try
            {
                const string sql = @"
 SELECT Id, TenantName, ApiKey, IsActive, TenantId, CreatedAt
  FROM TenantApiKeys
    WHERE ApiKey = @ApiKey
 LIMIT 1";

                return await _connection.QueryFirstOrDefaultAsync<TenantApiKey>(sql, new { ApiKey = apiKey });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant API key by key");
                throw;
            }
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                const string sql = "SELECT COUNT(1) FROM TenantApiKeys WHERE ApiKey = @ApiKey AND IsActive = 1";
                var count = await _connection.ExecuteScalarAsync<int>(sql, new { ApiKey = apiKey });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key");
                throw;
            }
        }
    }
}
