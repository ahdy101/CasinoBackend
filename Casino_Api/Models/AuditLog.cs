namespace Casino.Backend.Models
{
    /// <summary>
    /// System-wide audit log for compliance
    /// </summary>
    public class AuditLog
    {
  public long Id { get; set; }
   public string EntityType { get; set; } = string.Empty;  // "User", "Bet", "Payment", etc.
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty;  // "Create", "Update", "Delete", "Login"
        public int? UserId { get; set; }
  public int? AdminId { get; set; }
        public string? OldValues { get; set; }  // JSON
        public string? NewValues { get; set; }  // JSON
      public string? IpAddress { get; set; }
   public string? UserAgent { get; set; }
  public string? AdditionalData { get; set; }  // JSON for extra context
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Admin activity log
    /// </summary>
    public class AdminActivityLog
    {
 public int Id { get; set; }
    public int AdminId { get; set; }
        public string Action { get; set; } = string.Empty;
      public string Description { get; set; } = string.Empty;
     public string? TargetType { get; set; }  // "User", "Withdrawal", etc.
        public int? TargetId { get; set; }
        public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public AdminUser? Admin { get; set; }
    }

    /// <summary>
    /// User login history for security
    /// </summary>
    public class LoginHistory
    {
        public int Id { get; set; }
     public int UserId { get; set; }
public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
      public string? DeviceFingerprint { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
 public bool IsSuccessful { get; set; }
  public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
    }
}
