using System.ComponentModel.DataAnnotations;

namespace Casino_Api.Models;

public class TenantApiKey
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string TenantName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ApiKey { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
