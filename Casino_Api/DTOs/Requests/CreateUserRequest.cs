using System.ComponentModel.DataAnnotations;

namespace Casino.Backend.DTOs.Requests
{
    /// <summary>
    /// Request model for creating a new user
    /// </summary>
    public class CreateUserRequest
    {
   /// <summary>
     /// Desired username (must be unique)
        /// </summary>
      [Required(ErrorMessage = "Username is required")]
  [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
     [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
   public string Username { get; set; } = string.Empty;

     /// <summary>
        /// Password (will be hashed with BCrypt)
   /// </summary>
     [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Initial balance (optional, defaults to 1000)
      /// </summary>
  [Range(0, 1000000)]
        public decimal? InitialBalance { get; set; }
    }
}
