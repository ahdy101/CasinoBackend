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
     SELECT Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt, DeletedAt
         FROM AdminUsers
    WHERE Id = @Id AND IsDeleted = 0";

using var connection = _connectionFactory.CreateConnection();
    return await connection.QueryFirstOrDefaultAsync<AdminUser>(sql, new { Id = id });
 }

  public async Task<IEnumerable<AdminUser>> GetAllAsync()
   {
  const string sql = @"
   SELECT Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt, DeletedAt
 FROM AdminUsers
                WHERE IsDeleted = 0
  ORDER BY CreatedAt DESC";

     using var connection = _connectionFactory.CreateConnection();
 return await connection.QueryAsync<AdminUser>(sql);
   }

   public async Task<int> AddAsync(AdminUser entity)
   {
  const string sql = @"
 INSERT INTO AdminUsers (Username, Email, PasswordHash, Role, IsDeleted, CreatedAt)
        VALUES (@Username, @Email, @PasswordHash, @Role, @IsDeleted, @CreatedAt);
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
      Email = @Email,
        PasswordHash = @PasswordHash,
   Role = @Role
  WHERE Id = @Id AND IsDeleted = 0";

      using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, entity);
    return rowsAffected > 0;
 }

        public async Task<bool> DeleteAsync(int id)
        {
            // Soft delete - set IsDeleted = 1 and DeletedAt = NOW()
         const string sql = @"
       UPDATE AdminUsers 
      SET IsDeleted = 1, DeletedAt = @DeletedAt 
   WHERE Id = @Id AND IsDeleted = 0";

using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
      return rowsAffected > 0;
        }

        public async Task<AdminUser?> GetByUsernameAsync(string username)
        {
const string sql = @"
    SELECT Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt, DeletedAt
   FROM AdminUsers
   WHERE Username = @Username AND IsDeleted = 0
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

     /// <summary>
 /// Gets all admin users including soft-deleted ones
   /// </summary>
   public async Task<IEnumerable<AdminUser>> GetAllIncludingDeletedAsync()
      {
            const string sql = @"
   SELECT Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt, DeletedAt
              FROM AdminUsers
        ORDER BY CreatedAt DESC";

            using var connection = _connectionFactory.CreateConnection();
      return await connection.QueryAsync<AdminUser>(sql);
        }

        /// <summary>
      /// Restores a soft-deleted admin user
        /// </summary>
        public async Task<bool> RestoreAsync(int id)
        {
          const string sql = @"
         UPDATE AdminUsers 
      SET IsDeleted = 0, DeletedAt = NULL 
      WHERE Id = @Id AND IsDeleted = 1";

  using var connection = _connectionFactory.CreateConnection();
          var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
         return rowsAffected > 0;
        }

      /// <summary>
        /// Permanently deletes an admin user (hard delete)
        /// </summary>
        public async Task<bool> HardDeleteAsync(int id)
        {
        const string sql = "DELETE FROM AdminUsers WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
     return rowsAffected > 0;
        }
    }
}
