namespace Casino.Backend.Models
{
    /// <summary>
    /// Audit log for KYC access (GDPR compliance)
    /// </summary>
    public class KycAuditLog
    {
  public int Id { get; set; }
      public int UserId { get; set; }
 public int AdminId { get; set; }
     public string Action { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
