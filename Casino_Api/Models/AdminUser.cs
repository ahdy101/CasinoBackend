namespace Casino.Backend.Models
{
 public class AdminUser
 {
 public int Id { get; set; }
 public string Username { get; set; }
 public string Email { get; set; } = string.Empty;
 public string PasswordHash { get; set; }
 public string Role { get; set; } = "Admin";
 public bool IsDeleted { get; set; } = false;
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 public DateTime? DeletedAt { get; set; }
 }
}
