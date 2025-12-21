using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
    /// Dapper-based repository for AdminUser using raw SQL
    /// </summary>
    public class AdminUserRepository : IAdminUserRepository
    {
   private readonly IDbConnectionFactory _connectionFactory;

   public AdminUserRepository(IDbConnectionFactory connectionFactory)
        {
 _connectionFactory = connectionFactory;
        }

        public async Task<AdminUser?> GetByIdAsync(int id)
        {
       const string sql = @"
             SELECT Id, Username, PasswordHash, CreatedAt
                FROM AdminUsers
        WHERE Id = @Id";

using var connection = _connectionFactory.CreateConnection();
    return await connection.QueryFirstOrDefaultAsync<AdminUser>(sql, new { Id = id });
    }

        public async Task<IEnumerable<AdminUser>> GetAllAsync()
        {
  const string sql = @"
   SELECT Id, Username, PasswordHash, CreatedAt
    FROM AdminUsers
                ORDER BY CreatedAt DESC";

     using var connection = _connectionFactory.CreateConnection();
 return await connection.QueryAsync<AdminUser>(sql);
        }

        public async Task<int> AddAsync(AdminUser entity)
        {
  const string sql = @"
 INSERT INTO AdminUsers (Username, PasswordHash, CreatedAt)
        VALUES (@Username, @PasswordHash, @CreatedAt);
       SELECT LAST_INSERT_ID();";

            using var connection = _connectionFactory.CreateConnection();
            var id = await connection.ExecuteScalarAsync<int>(sql, entity);
            entity.Id = id;
  return id;
        }

        public async Task<bool> UpdateAsync(AdminUser entity)
   {
   const string sql = @"
      UPDATE AdminUsers
      SET Username = @Username,
        PasswordHash = @PasswordHash
      WHERE Id = @Id";

      using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, entity);
    return rowsAffected > 0;
 }

public async Task<bool> DeleteAsync(int id)
        {
       const string sql = "DELETE FROM AdminUsers WHERE Id = @Id";

          using var connection = _connectionFactory.CreateConnection();
    var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
      return rowsAffected > 0;
        }

        public async Task<AdminUser?> GetByUsernameAsync(string username)
        {
            const string sql = @"
    SELECT Id, Username, PasswordHash, CreatedAt
        FROM AdminUsers
         WHERE Username = @Username
    LIMIT 1";

  using var connection = _connectionFactory.CreateConnection();
       return await connection.QueryFirstOrDefaultAsync<AdminUser>(sql, new { Username = username });
        }

        public async Task<bool> UsernameExistsAsync(string username)
 {
            const string sql = "SELECT COUNT(1) FROM AdminUsers WHERE Username = @Username";

       using var connection = _connectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
            return count > 0;
        }
    }
}
