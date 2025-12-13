using System.ComponentModel.DataAnnotations;

namespace Casino_Api.Models;

public class AdminUser
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Admin";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
