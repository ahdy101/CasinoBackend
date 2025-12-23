using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
    public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
  {
        /// <summary>
 /// Get token by token string
   /// </summary>
     Task<PasswordResetToken?> GetByTokenAsync(string token);
 
    /// <summary>
 /// Get active (unused, not expired) token for user
        /// </summary>
     Task<PasswordResetToken?> GetActiveTokenByUserIdAsync(int userId);
        
        /// <summary>
        /// Mark all user's tokens as used
        /// </summary>
   Task InvalidateUserTokensAsync(int userId);
    }
}
