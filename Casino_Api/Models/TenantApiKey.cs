namespace Casino.Backend.Models
{
 public class TenantApiKey
 {
 public int Id { get; set; }
 public string TenantName { get; set; }
 public string ApiKey { get; set; }
 public bool IsActive { get; set; } = true;
 public int? TenantId { get; set; }
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 }
}