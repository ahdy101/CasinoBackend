using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;
using System.Data;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
    /// Dapper-based repository for User using raw SQL
    /// </summary>
    public class UserRepository : IUserRepository
 {
        private readonly IDbConnection _connection;
        private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbConnectionFactory connectionFactory, ILogger<UserRepository> logger)
        {
            _connection = connectionFactory.CreateConnection();
  _logger = logger;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
   try
     {
        const string sql = @"
         SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, LastActivityAt, TenantId
     FROM Users
           WHERE Id = @Id AND IsDeleted = 0";

    return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }
      catch (Exception ex)
      {
  _logger.LogError(ex, "Error getting user by ID: {Id}", id);
  throw;
   }
    }

 public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
      {
   const string sql = @"
      SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, LastActivityAt, TenantId
    FROM Users
      WHERE IsDeleted = 0
ORDER BY CreatedAt DESC";

    return await _connection.QueryAsync<User>(sql);
    }
     catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting all users");
   throw;
            }
      }

    public async Task<int> AddAsync(User entity)
      {
     try
        {
   const string sql = @"
        INSERT INTO Users (Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, LastActivityAt, TenantId)
   VALUES (@Username, @Email, @PasswordHash, @Balance, @Role, @IsDeleted, @CreatedAt, @ModifiedAt, @LastActivityAt, @TenantId);
          SELECT LAST_INSERT_ID();";

       var id = await _connection.ExecuteScalarAsync<int>(sql, entity);
       entity.Id = id;
     return id;
}
     catch (Exception ex)
     {
       _logger.LogError(ex, "Error adding user: {Username}", entity.Username);
        throw;
   }
        }

     public async Task<bool> UpdateAsync(User entity)
        {
         try
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

    var rowsAffected = await _connection.ExecuteAsync(sql, entity);
  return rowsAffected > 0;
    }
     catch (Exception ex)
            {
        _logger.LogError(ex, "Error updating user: {Id}", entity.Id);
    throw;
    }
  }

      public async Task<bool> DeleteAsync(int id)
        {
            try
            {
 const string sql = @"
   UPDATE Users 
      SET IsDeleted = 1, DeletedAt = @DeletedAt 
   WHERE Id = @Id AND IsDeleted = 0";

  var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
 return rowsAffected > 0;
   }
       catch (Exception ex)
            {
        _logger.LogError(ex, "Error deleting user: {Id}", id);
       throw;
}
     }

    public async Task<User?> GetByUsernameAsync(string username)
      {
    try
     {
    const string sql = @"
            SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, LastActivityAt, TenantId
 FROM Users
      WHERE Username = @Username AND IsDeleted = 0
 LIMIT 1";

      return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
 }
       catch (Exception ex)
 {
      _logger.LogError(ex, "Error getting user by username: {Username}", username);
                throw;
   }
        }

     public async Task<bool> UsernameExistsAsync(string username)
      {
          try
        {
     const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND IsDeleted = 0";
      var count = await _connection.ExecuteScalarAsync<int>(sql, new { Username = username });
     return count > 0;
   }
      catch (Exception ex)
 {
        _logger.LogError(ex, "Error checking username exists: {Username}", username);
      throw;
     }
  }

        public async Task<User?> GetByEmailAsync(string email)
      {
 try
 {
   const string sql = @"
          SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, LastActivityAt, TenantId
       FROM Users
    WHERE Email = @Email AND IsDeleted = 0
   LIMIT 1";

         return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
  }
    catch (Exception ex)
     {
      _logger.LogError(ex, "Error getting user by email: {Email}", email);
      throw;
         }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
    try
 {
 const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND IsDeleted = 0";
     var count = await _connection.ExecuteScalarAsync<int>(sql, new { Email = email });
    return count > 0;
          }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error checking email exists: {Email}", email);
   throw;
 }
   }

      public async Task<decimal> GetBalanceAsync(int userId)
    {
 try
            {
 const string sql = "SELECT Balance FROM Users WHERE Id = @UserId AND IsDeleted = 0";
     return await _connection.ExecuteScalarAsync<decimal>(sql, new { UserId = userId });
   }
   catch (Exception ex)
       {
  _logger.LogError(ex, "Error getting balance for user: {UserId}", userId);
 throw;
        }
    }

 public async Task UpdateBalanceAsync(int userId, decimal newBalance)
  {
   try
 {
 const string sql = @"
   UPDATE Users 
    SET Balance = @Balance, ModifiedAt = @ModifiedAt 
      WHERE Id = @UserId AND IsDeleted = 0";

    await _connection.ExecuteAsync(sql, new { UserId = userId, Balance = newBalance, ModifiedAt = DateTime.UtcNow });
    }
         catch (Exception ex)
 {
      _logger.LogError(ex, "Error updating balance for user: {UserId}", userId);
    throw;
         }
     }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
  try
  {
  const string sql = @"
  SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, LastActivityAt, TenantId
    FROM Users
       WHERE Role = @Role AND IsDeleted = 0
   ORDER BY CreatedAt DESC";

  return await _connection.QueryAsync<User>(sql, new { Role = role });
   }
   catch (Exception ex)
      {
    _logger.LogError(ex, "Error getting users by role: {Role}", role);
     throw;
  }
   }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(int minutesThreshold = 15)
     {
   try
    {
 const string sql = @"
     SELECT Id, Username, Email, PasswordHash, Balance, Role, IsDeleted, CreatedAt, ModifiedAt, DeletedAt, LastActivityAt, TenantId
        FROM Users
       WHERE IsDeleted = 0 
        AND LastActivityAt >= @Threshold
    ORDER BY LastActivityAt DESC";

       var threshold = DateTime.UtcNow.AddMinutes(-minutesThreshold);
    return await _connection.QueryAsync<User>(sql, new { Threshold = threshold });
  }
  catch (Exception ex)
{
    _logger.LogError(ex, "Error getting active users");
   throw;
   }
 }

     public async Task UpdateLastActivityAsync(int userId)
        {
    try
            {
         const string sql = "UPDATE Users SET LastActivityAt = @Now WHERE Id = @UserId";
    await _connection.ExecuteAsync(sql, new { UserId = userId, Now = DateTime.UtcNow });
   }
        catch (Exception ex)
 {
 _logger.LogError(ex, "Error updating last activity for user: {UserId}", userId);
      throw;
     }
        }
 }
}
