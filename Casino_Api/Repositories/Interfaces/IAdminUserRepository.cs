using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
    /// <summary>
 /// Repository interface for AdminUser operations
 /// </summary>
    public interface IAdminUserRepository : IRepository<AdminUser>
    {
      Task<AdminUser?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
  
   /// <summary>
        /// Gets all admin users including soft-deleted ones
        /// </summary>
      Task<IEnumerable<AdminUser>> GetAllIncludingDeletedAsync();
        
      /// <summary>
        /// Restores a soft-deleted admin user
 /// </summary>
        Task<bool> RestoreAsync(int id);
        
      /// <summary>
 /// Permanently deletes an admin user (hard delete)
      /// </summary>
  Task<bool> HardDeleteAsync(int id);
    }
}
