using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;
using System.Data;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
  /// Dapper-based repository for Bet using raw SQL
    /// </summary>
    public class BetRepository : IBetRepository
  {
        private readonly IDbConnection _connection;
        private readonly ILogger<BetRepository> _logger;

   public BetRepository(IDbConnectionFactory connectionFactory, ILogger<BetRepository> logger)
        {
     _connection = connectionFactory.CreateConnection();
      _logger = logger;
      }

        public async Task<Bet?> GetByIdAsync(int id)
        {
        try
 {
            const string sql = @"
     SELECT Id, UserId, Game, Amount, Choice, Payout, CreatedAt
        FROM Bets
   WHERE Id = @Id";

              return await _connection.QueryFirstOrDefaultAsync<Bet>(sql, new { Id = id });
 }
            catch (Exception ex)
    {
    _logger.LogError(ex, "Error getting bet by ID: {Id}", id);
      throw;
      }
        }

        public async Task<IEnumerable<Bet>> GetAllAsync()
        {
      try
            {
 const string sql = @"
     SELECT Id, UserId, Game, Amount, Choice, Payout, CreatedAt
         FROM Bets
        ORDER BY CreatedAt DESC";

           return await _connection.QueryAsync<Bet>(sql);
            }
  catch (Exception ex)
            {
         _logger.LogError(ex, "Error getting all bets");
      throw;
    }
  }

      public async Task<int> AddAsync(Bet entity)
     {
    try
    {
         const string sql = @"
      INSERT INTO Bets (UserId, Game, Amount, Choice, Payout, CreatedAt)
        VALUES (@UserId, @Game, @Amount, @Choice, @Payout, @CreatedAt);
SELECT LAST_INSERT_ID();";

    var id = await _connection.ExecuteScalarAsync<int>(sql, entity);
entity.Id = id;
return id;
            }
            catch (Exception ex)
 {
     _logger.LogError(ex, "Error adding bet for user: {UserId}", entity.UserId);
        throw;
  }
        }

        public async Task<bool> UpdateAsync(Bet entity)
        {
            try
      {
    const string sql = @"
               UPDATE Bets
     SET UserId = @UserId,
      Game = @Game,
      Amount = @Amount,
         Choice = @Choice,
          Payout = @Payout
            WHERE Id = @Id";

     var rowsAffected = await _connection.ExecuteAsync(sql, entity);
      return rowsAffected > 0;
    }
    catch (Exception ex)
          {
           _logger.LogError(ex, "Error updating bet: {Id}", entity.Id);
     throw;
        }
 }

        public async Task<bool> DeleteAsync(int id)
 {
            try
 {
                const string sql = "DELETE FROM Bets WHERE Id = @Id";
   var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id });
      return rowsAffected > 0;
    }
            catch (Exception ex)
            {
  _logger.LogError(ex, "Error deleting bet: {Id}", id);
  throw;
  }
        }

        public async Task<IEnumerable<Bet>> GetBetsByUserIdAsync(int userId)
      {
    try
            {
     const string sql = @"
   SELECT Id, UserId, Game, Amount, Choice, Payout, CreatedAt
        FROM Bets
       WHERE UserId = @UserId
        ORDER BY CreatedAt DESC";

          return await _connection.QueryAsync<Bet>(sql, new { UserId = userId });
 }
            catch (Exception ex)
        {
        _logger.LogError(ex, "Error getting bets for user: {UserId}", userId);
                throw;
   }
        }

        public async Task<Bet?> GetBetWithUserAsync(int betId)
      {
      try
       {
            const string sql = @"
        SELECT b.Id, b.UserId, b.Game, b.Amount, b.Choice, b.Payout, b.CreatedAt
             FROM Bets b
        WHERE b.Id = @BetId";

        return await _connection.QueryFirstOrDefaultAsync<Bet>(sql, new { BetId = betId });
       }
            catch (Exception ex)
 {
             _logger.LogError(ex, "Error getting bet with user: {BetId}", betId);
  throw;
}
        }

        public async Task UpdatePayoutAsync(int betId, decimal payout)
     {
     try
  {
  const string sql = "UPDATE Bets SET Payout = @Payout WHERE Id = @BetId";
    await _connection.ExecuteAsync(sql, new { BetId = betId, Payout = payout });
    }
      catch (Exception ex)
{
          _logger.LogError(ex, "Error updating payout for bet: {BetId}", betId);
         throw;
     }
      }
    }
}
