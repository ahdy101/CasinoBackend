using Casino.Backend.Enums;

namespace Casino.Backend.Models
{
    /// <summary>
    /// Payment transaction record for deposits and withdrawals
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        /// <summary>
  /// Payment type (Deposit or Withdrawal)
        /// </summary>
   public PaymentType Type { get; set; }
   
        /// <summary>
        /// Payment amount in USD
   /// </summary>
     public decimal Amount { get; set; }
        
     /// <summary>
        /// Payment provider (Stripe, PayPal, Crypto, etc.)
        /// </summary>
        public PaymentProvider Provider { get; set; }
 
        /// <summary>
  /// Payment method (CreditCard, DebitCard, BankTransfer, etc.)
        /// </summary>
        public PaymentMethod Method { get; set; }
  
 /// <summary>
        /// Current status of the payment
        /// </summary>
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        
  /// <summary>
        /// External transaction ID from payment provider
        /// </summary>
        public string? ExternalTransactionId { get; set; }
        
        /// <summary>
/// Reference number for internal tracking
        /// </summary>
     public string ReferenceNumber { get; set; } = string.Empty;
        
   /// <summary>
/// Related wallet transaction ID
  /// </summary>
  public int? WalletTransactionId { get; set; }
 public WalletTransaction? WalletTransaction { get; set; }
        
        /// <summary>
        /// Currency code (USD, EUR, BTC, etc.)
  /// </summary>
      public string Currency { get; set; } = "USD";
        
  /// <summary>
     /// Payment processing fee
     /// </summary>
   public decimal Fee { get; set; }
        
/// <summary>
   /// Net amount after fees
        /// </summary>
        public decimal NetAmount { get; set; }
  
   /// <summary>
    /// IP address from which payment was initiated
        /// </summary>
   public string? IpAddress { get; set; }
        
        /// <summary>
        /// Additional metadata (JSON)
        /// </summary>
  public string? Metadata { get; set; }
      
        /// <summary>
   /// Error message if payment failed
        /// </summary>
 public string? ErrorMessage { get; set; }
        
  /// <summary>
   /// When payment was initiated
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      
   /// <summary>
    /// When payment was processed/completed
     /// </summary>
      public DateTime? CompletedAt { get; set; }
   
        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
