using System.ComponentModel.DataAnnotations;

namespace Casino.Backend.DTOs.Requests
{
    /// <summary>
    /// Request model for updating user information
    /// </summary>
    public class UpdateUserRequest
    {
      /// <summary>
 /// User ID to update
     /// </summary>
        [Required]
        public int Id { get; set; }

   /// <summary>
        /// New username (optional)
        /// </summary>
  [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string? Username { get; set; }

      /// <summary>
        /// New password (optional, will be hashed with BCrypt if provided)
        /// </summary>
   [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? NewPassword { get; set; }
    }
}
