using System.ComponentModel.DataAnnotations;

namespace Casino.Backend.DTOs.Requests
{
    public class ForgotPasswordRequest
    {
 [Required(ErrorMessage = "Email is required")]
   [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
   [MinLength(3)]
        [MaxLength(50)]
        public string? Username { get; set; }

[EmailAddress]
 [MaxLength(100)]
   public string? Email { get; set; }
    }

    public class ChangePasswordRequest
    {
    [Required(ErrorMessage = "Current password is required")]
      public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
   [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
  public string NewPassword { get; set; } = string.Empty;
    }
}
