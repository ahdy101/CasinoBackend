namespace Casino.Backend.Models
{
    /// <summary>
    /// Withdrawal request from user
    /// </summary>
    public class WithdrawalRequest
  {
     public int Id { get; set; }
      public int UserId { get; set; }
   public decimal Amount { get; set; }
     public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; } = string.Empty;// "bank", "crypto", "paypal"
        public string? PaymentDetails { get; set; }  // JSON with account details
        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
      public int? ProcessedByAdminId { get; set; }
  public string? RejectionReason { get; set; }
   public string? TransactionReference { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
  public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

   // Navigation
      public User? User { get; set; }
    }

    public enum WithdrawalStatus
    {
        Pending = 1,
     Processing = 2,
      Approved = 3,
   Rejected = 4,
   Completed = 5,
        Cancelled = 6
    }
}
