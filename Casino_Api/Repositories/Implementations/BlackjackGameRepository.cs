using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
    /// Dapper-based repository for BlackjackGame using raw SQL
    /// </summary>
    public class BlackjackGameRepository : IBlackjackGameRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public BlackjackGameRepository(IDbConnectionFactory connectionFactory)
    {
      _connectionFactory = connectionFactory;
        }

        public async Task<BlackjackGame?> GetByIdAsync(int id)
        {
 const string sql = @"
        SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
         PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
  FROM BlackjackGames
    WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<BlackjackGame>(sql, new { Id = id });
        }

        public async Task<IEnumerable<BlackjackGame>> GetAllAsync()
        {
         const string sql = @"
     SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
     PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
  FROM BlackjackGames
      ORDER BY CreatedAt DESC";

       using var connection = _connectionFactory.CreateConnection();
         return await connection.QueryAsync<BlackjackGame>(sql);
        }

      public async Task<int> AddAsync(BlackjackGame entity)
   {
            const string sql = @"
   INSERT INTO BlackjackGames 
              (UserId, BetAmount, PlayerCards, DealerCards, PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt)
         VALUES 
          (@UserId, @BetAmount, @PlayerCards, @DealerCards, @PlayerTotal, @DealerTotal, @Status, @Payout, @CreatedAt, @CompletedAt);
   SELECT LAST_INSERT_ID();";

      using var connection = _connectionFactory.CreateConnection();
        var id = await connection.ExecuteScalarAsync<int>(sql, entity);
   entity.Id = id;
            return id;
        }

        public async Task<bool> UpdateAsync(BlackjackGame entity)
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

            using var connection = _connectionFactory.CreateConnection();
      var rowsAffected = await connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
     {
            const string sql = "DELETE FROM BlackjackGames WHERE Id = @Id";

  using var connection = _connectionFactory.CreateConnection();
      var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
return rowsAffected > 0;
        }

 public async Task<IEnumerable<BlackjackGame>> GetGamesByUserIdAsync(int userId)
    {
            const string sql = @"
    SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
    PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
  FROM BlackjackGames
          WHERE UserId = @UserId
 ORDER BY CreatedAt DESC";

          using var connection = _connectionFactory.CreateConnection();
   return await connection.QueryAsync<BlackjackGame>(sql, new { UserId = userId });
        }

        public async Task<BlackjackGame?> GetActiveGameAsync(int userId)
   {
 const string sql = @"
    SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
           PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
      FROM BlackjackGames
     WHERE UserId = @UserId AND Status = 'Active'
        LIMIT 1";

          using var connection = _connectionFactory.CreateConnection();
     return await connection.QueryFirstOrDefaultAsync<BlackjackGame>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<BlackjackGame>> GetCompletedGamesAsync(int userId, int limit = 10)
        {
         const string sql = @"
SELECT Id, UserId, BetAmount, PlayerCards, DealerCards, 
  PlayerTotal, DealerTotal, Status, Payout, CreatedAt, CompletedAt
      FROM BlackjackGames
WHERE UserId = @UserId AND CompletedAt IS NOT NULL
         ORDER BY CompletedAt DESC
         LIMIT @Limit";

 using var connection = _connectionFactory.CreateConnection();
     return await connection.QueryAsync<BlackjackGame>(sql, new { UserId = userId, Limit = limit });
   }
    }
}
