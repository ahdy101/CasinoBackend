using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
    /// Dapper-based repository for User using raw SQL
    /// </summary>
    public class UserRepository : IUserRepository
    {
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
 {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
 const string sql = @"
   SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, TenantId
      FROM Users
   WHERE Id = @Id AND IsDeleted = 0";

 using var connection = _connectionFactory.CreateConnection();
    return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
 }

  public async Task<IEnumerable<User>> GetAllAsync()
        {
   const string sql = @"
       SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, TenantId
        FROM Users
        WHERE IsDeleted = 0
      ORDER BY CreatedAt DESC";

  using var connection = _connectionFactory.CreateConnection();
    return await connection.QueryAsync<User>(sql);
  }

     public async Task<int> AddAsync(User entity)
   {
       const string sql = @"
INSERT INTO Users (Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, TenantId)
VALUES (@Username, @Email, @PasswordHash, @Balance, @Role, @IsDeleted, @CreatedAt, @TenantId);
  SELECT LAST_INSERT_ID();";

     using var connection = _connectionFactory.CreateConnection();
         var id = await connection.ExecuteScalarAsync<int>(sql, entity);
    entity.Id = id;
       return id;
 }

    public async Task<bool> UpdateAsync(User entity)
    {
            entity.ModifiedAt = DateTime.UtcNow;
 
  const string sql = @"
UPDATE Users
   SET Username = @Username,
      Email = @Email,
PasswordHash = @PasswordHash,
    Balance = @Balance,
       Role = @Role,
            ModifiedAt = @ModifiedAt,
   TenantId = @TenantId
     WHERE Id = @Id AND IsDeleted = 0";

 using var connection = _connectionFactory.CreateConnection();
      var rowsAffected = await connection.ExecuteAsync(sql, entity);
    return rowsAffected > 0;
        }

      public async Task<bool> DeleteAsync(int id)
        {
         const string sql = @"
     UPDATE Users 
       SET IsDeleted = 1, DeletedAt = @DeletedAt 
        WHERE Id = @Id AND IsDeleted = 0";

    using var connection = _connectionFactory.CreateConnection();
 var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
      return rowsAffected > 0;
     }

   public async Task<User?> GetByUsernameAsync(string username)
   {
      const string sql = @"
     SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, TenantId
     FROM Users
    WHERE Username = @Username AND IsDeleted = 0
           LIMIT 1";

   using var connection = _connectionFactory.CreateConnection();
      return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
  }

        public async Task<bool> UsernameExistsAsync(string username)
        {
       const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND IsDeleted = 0";

  using var connection = _connectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
            return count > 0;
        }

        public async Task<User?> GetByEmailAsync(string email)
     {
   const string sql = @"
    SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, TenantId
     FROM Users
       WHERE Email = @Email AND IsDeleted = 0
                LIMIT 1";

            using var connection = _connectionFactory.CreateConnection();
     return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<bool> EmailExistsAsync(string email)
    {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND IsDeleted = 0";

   using var connection = _connectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<decimal> GetBalanceAsync(int userId)
     {
   const string sql = "SELECT Balance FROM Users WHERE Id = @UserId AND IsDeleted = 0";

         using var connection = _connectionFactory.CreateConnection();
    return await connection.ExecuteScalarAsync<decimal>(sql, new { UserId = userId });
      }

        public async Task UpdateBalanceAsync(int userId, decimal newBalance)
        {
            const string sql = @"
          UPDATE Users 
            SET Balance = @Balance, ModifiedAt = @ModifiedAt 
       WHERE Id = @UserId AND IsDeleted = 0";

    using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { UserId = userId, Balance = newBalance, ModifiedAt = DateTime.UtcNow });
        }
    }
}
