using Casino.Backend.Data;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Dapper;

namespace Casino.Backend.Repositories.Implementations
{
    /// <summary>
  /// Dapper-based repository for Bet using raw SQL
    /// </summary>
    public class BetRepository : IBetRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public BetRepository(IDbConnectionFactory connectionFactory)
        {
    _connectionFactory = connectionFactory;
        }

        public async Task<Bet?> GetByIdAsync(int id)
        {
       const string sql = @"
    SELECT Id, UserId, Game, Amount, Choice, Payout, CreatedAt
 FROM Bets
  WHERE Id = @Id";

   using var connection = _connectionFactory.CreateConnection();
          return await connection.QueryFirstOrDefaultAsync<Bet>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Bet>> GetAllAsync()
   {
  const string sql = @"
       SELECT Id, UserId, Game, Amount, Choice, Payout, CreatedAt
       FROM Bets
  ORDER BY CreatedAt DESC";

  using var connection = _connectionFactory.CreateConnection();
          return await connection.QueryAsync<Bet>(sql);
}

        public async Task<int> AddAsync(Bet entity)
   {
          const string sql = @"
             INSERT INTO Bets (UserId, Game, Amount, Choice, Payout, CreatedAt)
  VALUES (@UserId, @Game, @Amount, @Choice, @Payout, @CreatedAt);
                SELECT LAST_INSERT_ID();";

   using var connection = _connectionFactory.CreateConnection();
  var id = await connection.ExecuteScalarAsync<int>(sql, entity);
    entity.Id = id;
            return id;
    }

        public async Task<bool> UpdateAsync(Bet entity)
        {
      const string sql = @"
          UPDATE Bets
    SET UserId = @UserId,
      Game = @Game,
           Amount = @Amount,
        Choice = @Choice,
   Payout = @Payout
                WHERE Id = @Id";

    using var connection = _connectionFactory.CreateConnection();
    var rowsAffected = await connection.ExecuteAsync(sql, entity);
     return rowsAffected > 0;
        }

 public async Task<bool> DeleteAsync(int id)
      {
 const string sql = "DELETE FROM Bets WHERE Id = @Id";

   using var connection = _connectionFactory.CreateConnection();
          var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
      return rowsAffected > 0;
 }

     public async Task<IEnumerable<Bet>> GetBetsByUserIdAsync(int userId)
        {
            const string sql = @"
       SELECT Id, UserId, Game, Amount, Choice, Payout, CreatedAt
     FROM Bets
     WHERE UserId = @UserId
       ORDER BY CreatedAt DESC";

  using var connection = _connectionFactory.CreateConnection();
    return await connection.QueryAsync<Bet>(sql, new { UserId = userId });
        }

  public async Task<Bet?> GetBetWithUserAsync(int betId)
     {
  const string sql = @"
     SELECT b.Id, b.UserId, b.Game, b.Amount, b.Choice, b.Payout, b.CreatedAt
     FROM Bets b
    WHERE b.Id = @BetId";

     using var connection = _connectionFactory.CreateConnection();
  return await connection.QueryFirstOrDefaultAsync<Bet>(sql, new { BetId = betId });
  }

        public async Task UpdatePayoutAsync(int betId, decimal payout)
        {
        const string sql = "UPDATE Bets SET Payout = @Payout WHERE Id = @BetId";

  using var connection = _connectionFactory.CreateConnection();
await connection.ExecuteAsync(sql, new { BetId = betId, Payout = payout });
     }
    }
}
