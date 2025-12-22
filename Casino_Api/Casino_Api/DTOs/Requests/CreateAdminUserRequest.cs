using System.ComponentModel.DataAnnotations;

namespace Casino.Backend.DTOs.Requests
{
    /// <summary>
    /// Request model for creating a new admin user
    /// </summary>
    public class CreateAdminUserRequest
    {
        /// <summary>
        /// Admin username (must be unique)
     /// </summary>
      [Required(ErrorMessage = "Username is required")]
  [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; } = string.Empty;

     /// <summary>
    /// Admin email address (must be unique)
   /// </summary>
        [Required(ErrorMessage = "Email is required")]
 [EmailAddress(ErrorMessage = "Invalid email format")]
   [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

   /// <summary>
        /// Password (will be hashed with BCrypt)
      /// </summary>
        [Required(ErrorMessage = "Password is required")]
   [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
      /// Role for the admin user (defaults to "Admin")
        /// </summary>
        [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        public string Role { get; set; } = "Admin";
    }
}
