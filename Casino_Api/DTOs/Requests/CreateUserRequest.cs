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
        /// <example>player123</example>
        [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public string Username { get; set; } = "player123";

      /// <summary>
        /// Email address (must be unique)
     /// </summary>
  /// <example>player@example.com</example>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
  public string Email { get; set; } = "player@example.com";

    /// <summary>
        /// Password (will be hashed with BCrypt)
     /// Must be at least 8 characters with uppercase, lowercase, number, and special character
        /// </summary>
  /// <example>Password123!</example>
        [Required(ErrorMessage = "Password is required")]
     [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
   [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (@$!%*?&)")]
    public string Password { get; set; } = "Password123!";
  }
}
