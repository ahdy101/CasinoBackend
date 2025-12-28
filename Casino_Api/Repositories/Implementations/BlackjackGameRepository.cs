using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;
using System.Data;

namespace Casino.Backend.Repositories.Implementations
{
    public class BlackjackGameRepository : IBlackjackGameRepository
    {
     private readonly IDbConnection _connection;
        private readonly ILogger<BlackjackGameRepository> _logger;

        public BlackjackGameRepository(IDbConnectionFactory connectionFactory, ILogger<BlackjackGameRepository> logger)
        {
      _connection = connectionFactory.CreateConnection();
            _logger = logger;
 }

        public async Task<BlackjackGame?> GetByIdAsync(int id)
     {
      try
      {
          const string sql = @"
    SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
   PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
      FROM BlackjackGames
         WHERE Id = @Id";

   return await _connection.QueryFirstOrDefaultAsync<BlackjackGame>(sql, new { Id = id });
            }
            catch (Exception ex)
{
   _logger.LogError(ex, "Error getting blackjack game by ID: {Id}", id);
       throw;
       }
   }

        public async Task<IEnumerable<BlackjackGame>> GetAllAsync()
        {
       try
{
         const string sql = @"
      SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
      PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
        FROM BlackjackGames
        ORDER BY CreatedAt DESC";

           return await _connection.QueryAsync<BlackjackGame>(sql);
}
  catch (Exception ex)
         {
          _logger.LogError(ex, "Error getting all blackjack games");
                throw;
 }
        }

    public async Task<int> AddAsync(BlackjackGame entity)
        {
   try
      {
                const string sql = @"
       INSERT INTO BlackjackGames 
          (UserId, BetAmount, PlayerCards, DealerCards, PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt)
      VALUES 
      (@UserId, @BetAmount, @PlayerCards, @DealerCards, @PlayerTotal, @DealerTotal, @Status, @Payout, @CreatedAt, @CompletedAt);
    SELECT LAST_INSERT_ID();";

            var id = await _connection.ExecuteScalarAsync<int>(sql, entity);
         entity.Id = id;
            return id;
 }
            catch (Exception ex)
            {
    _logger.LogError(ex, "Error adding blackjack game for user: {UserId}", entity.UserId);
   throw;
            }
        }

        public async Task<bool> UpdateAsync(BlackjackGame entity)
        {
         try
            {
 const string sql = @"
     UPDATE BlackjackGames
            SET UserId = @UserId,
          BetAmount = @BetAmount,
       PlayerCards = @PlayerCards,
     DealerCards = @DealerCards,
             PlayerTotal = @PlayerTotal,
    DealerTotal = @DealerTotal,
  Status = @Status,
         Payout = @Payout,
 CompletedAt = @CompletedAt
        WHERE Id = @Id";

         var rowsAffected = await _connection.ExecuteAsync(sql, entity);
        return rowsAffected > 0;
 }
            catch (Exception ex)
     {
        _logger.LogError(ex, "Error updating blackjack game: {Id}", entity.Id);
    throw;
        }
  }

        public async Task<bool> DeleteAsync(int id)
        {
  try
    {
         const string sql = "DELETE FROM BlackjackGames WHERE Id = @Id";
        var rowsAffected = await _connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
  }
         catch (Exception ex)
   {
             _logger.LogError(ex, "Error deleting blackjack game: {Id}", id);
              throw;
     }
    }

        public async Task<IEnumerable<BlackjackGame>> GetGamesByUserIdAsync(int userId)
        {
       try
       {
         const string sql = @"
                    SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
              PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
           FROM BlackjackGames
             WHERE UserId = @UserId
               ORDER BY CreatedAt DESC";

                return await _connection.QueryAsync<BlackjackGame>(sql, new { UserId = userId });
      }
            catch (Exception ex)
      {
       _logger.LogError(ex, "Error getting blackjack games for user: {UserId}", userId);
  throw;
            }
        }

      public async Task<BlackjackGame?> GetActiveGameAsync(int userId)
        {
      try
       {
       const string sql = @"
             SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
              PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
           FROM BlackjackGames
          WHERE UserId = @UserId AND Status = 'Active'
       LIMIT 1";

     return await _connection.QueryFirstOrDefaultAsync<BlackjackGame>(sql, new { UserId = userId });
      }
   catch (Exception ex)
       {
     _logger.LogError(ex, "Error getting active game for user: {UserId}", userId);
  throw;
            }
     }

        public async Task<IEnumerable<BlackjackGame>> GetCompletedGamesAsync(int userId, int limit = 10)
        {
            try
         {
 const string sql = @"
   SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
           PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
     FROM BlackjackGames
WHERE UserId = @UserId AND CompletedAt IS NOT NULL
                    ORDER BY CompletedAt DESC
   LIMIT @Limit";

             return await _connection.QueryAsync<BlackjackGame>(sql, new { UserId = userId, Limit = limit });
    }
          catch (Exception ex)
            {
  _logger.LogError(ex, "Error getting completed games for user: {UserId}", userId);
    throw;
     }
        }
    }
}
