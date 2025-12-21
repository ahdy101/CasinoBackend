using MySqlConnector;

namespace Casino.Backend.Data
{
    /// <summary>
    /// Implementation of database connection factory
    /// </summary>
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
          _connectionString = configuration.GetConnectionString("DefaultConnection") 
           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

  public MySqlConnection CreateConnection()
        {
       return new MySqlConnection(_connectionString);
     }
    }
}
