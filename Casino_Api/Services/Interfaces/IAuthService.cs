using Casino.Backend.Models;

namespace Casino.Backend.Services.Interfaces
{
    /// <summary>
    /// Authentication service for user registration and login
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Register a new user with hashed password
        /// </summary>
        /// <param name="username">Unique username</param>
        /// <param name="password">Plain text password (will be hashed)</param>
  /// <param name="initialBalance">Starting balance (default: 1000)</param>
        /// <returns>Created user</returns>
        Task<User> RegisterAsync(string username, string password, decimal initialBalance = 1000m);

        /// <summary>
        /// Authenticate user and generate JWT token
        /// </summary>
        /// <param name="username">Username</param>
     /// <param name="password">Plain text password</param>
   /// <returns>JWT token string</returns>
 Task<string> LoginAsync(string username, string password);

        /// <summary>
        /// Verify if a username is already taken
    /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>True if available, false if taken</returns>
        Task<bool> IsUsernameAvailable(string username);

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="currentPassword">Current password for verification</param>
    /// <param name="newPassword">New password (will be hashed)</param>
  /// <returns>True if successful</returns>
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
