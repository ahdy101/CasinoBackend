using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;
using System.Data;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
    /// Dapper-based repository for AdminUser using raw SQL
    /// </summary>
    public class AdminUserRepository : IAdminUserRepository
    {
 private readonly IDbConnection _connection;
        private readonly ILogger<AdminUserRepository> _logger;

        public AdminUserRepository(IDbConnectionFactory connectionFactory, ILogger<AdminUserRepository> logger)
        {
      _connection = connectionFactory.CreateConnection();
            _logger = logger;
        }

        public async Task<AdminUser?> GetByIdAsync(int id)
        {
    try
     {
         const string sql = @"
            SELECT Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt, DeletedAt
    FROM AdminUsers
       WHERE Id = @Id AND IsDeleted = 0";

     return await _connection.QueryFirstOrDefaultAsync<AdminUser>(sql, new { Id = id });
       }
     catch (Exception ex)
{
           _logger.LogError(ex, "Error getting admin user by ID: {Id}", id);
  throw;
            }
        }

        public async Task<IEnumerable<AdminUser>> GetAllAsync()
        {
          try
        {
    const string sql = @"
        SELECT Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt, DeletedAt
        FROM AdminUsers
                  WHERE IsDeleted = 0
   ORDER BY CreatedAt DESC";

 return await _connection.QueryAsync<AdminUser>(sql);
            }
            catch (Exception ex)
            {
    _logger.LogError(ex, "Error getting all admin users");
           throw;
          }
        }

        public async Task<int> AddAsync(AdminUser entity)
   {
            try
   {
       const string sql = @"
   INSERT INTO AdminUsers (Username, Email, PasswordHash, Role, IsDeleted, CreatedAt)
      VALUES (@Username, @Email, @PasswordHash, @Role, @IsDeleted, @CreatedAt);
          SELECT LAST_INSERT_ID();";

       var id = await _connection.ExecuteScalarAsync<int>(sql, entity);
        entity.Id = id;
 return id;
    }
     catch (Exception ex)
            {
 _logger.LogError(ex, "Error adding admin user: {Username}", entity.Username);
  throw;
            }
    }

        public async Task<bool> UpdateAsync(AdminUser entity)
   {
            try
   {
       const string sql = @"
           UPDATE AdminUsers
              SET Username = @Username,
        Email = @Email,
          PasswordHash = @PasswordHash,
               Role = @Role
          WHERE Id = @Id AND IsDeleted = 0";

     var rowsAffected = await _connection.ExecuteAsync(sql, entity);
             return rowsAffected > 0;
            }
         catch (Exception ex)
  {
 _logger.LogError(ex, "Error updating admin user: {Id}", entity.Id);
           throw;
            }
        }

     public async Task<bool> DeleteAsync(int id)
   {
            try
            {
 const string sql = @"
 UPDATE AdminUsers 
            SET IsDeleted = 1, DeletedAt = @DeletedAt 
          WHERE Id = @Id AND IsDeleted = 0";

   var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
      return rowsAffected > 0;
            }
catch (Exception ex)
 {
_logger.LogError(ex, "Error deleting admin user: {Id}", id);
                throw;
            }
    }

        public async Task<AdminUser?> GetByUsernameAsync(string username)
    {
            try
        {
  const string sql = @"
              SELECT Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt, DeletedAt
  FROM AdminUsers
          WHERE Username = @Username AND IsDeleted = 0
   LIMIT 1";

  return await _connection.QueryFirstOrDefaultAsync<AdminUser>(sql, new { Username = username });
       }
 catch (Exception ex)
        {
           _logger.LogError(ex, "Error getting admin user by username: {Username}", username);
throw;
    }
}

        public async Task<bool> UsernameExistsAsync(string username)
        {
   try
            {
   const string sql = "SELECT COUNT(1) FROM AdminUsers WHERE Username = @Username";
           var count = await _connection.ExecuteScalarAsync<int>(sql, new { Username = username });
      return count > 0;
            }
      catch (Exception ex)
 {
            _logger.LogError(ex, "Error checking admin username exists: {Username}", username);
              throw;
  }
        }

        public async Task<IEnumerable<AdminUser>> GetAllIncludingDeletedAsync()
        {
    try
         {
 const string sql = @"
         SELECT Id, Username, Email, PasswordHash, Role, IsDeleted, CreatedAt, DeletedAt
 FROM AdminUsers
           ORDER BY CreatedAt DESC";

           return await _connection.QueryAsync<AdminUser>(sql);
  }
  catch (Exception ex)
  {
    _logger.LogError(ex, "Error getting all admin users including deleted");
          throw;
      }
    }

  public async Task<bool> RestoreAsync(int id)
        {
   try
            {
     const string sql = @"
        UPDATE AdminUsers 
        SET IsDeleted = 0, DeletedAt = NULL 
        WHERE Id = @Id AND IsDeleted = 1";

                var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id });
    return rowsAffected > 0;
            }
     catch (Exception ex)
   {
              _logger.LogError(ex, "Error restoring admin user: {Id}", id);
  throw;
            }
  }

        public async Task<bool> HardDeleteAsync(int id)
        {
            try
    {
       const string sql = "DELETE FROM AdminUsers WHERE Id = @Id";
                var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id });
         return rowsAffected > 0;
            }
            catch (Exception ex)
  {
   _logger.LogError(ex, "Error hard deleting admin user: {Id}", id);
          throw;
            }
        }
    }
}
