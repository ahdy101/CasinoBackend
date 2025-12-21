using MySqlConnector;

namespace Casino.Backend.Data
{
    /// <summary>
    /// Factory for creating database connections
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
/// Create a new MySQL database connection
/// </summary>
  MySqlConnection CreateConnection();
    }
}
