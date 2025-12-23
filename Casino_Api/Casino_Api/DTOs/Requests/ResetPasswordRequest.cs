using System.ComponentModel.DataAnnotations;

namespace Casino.Backend.DTOs.Requests
{
    /// <summary>
    /// Request to reset password using token from email
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// Reset token from email link
        /// </summary>
     [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
   /// New password to set
      /// </summary>
    [Required(ErrorMessage = "New password is required")]
   [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
