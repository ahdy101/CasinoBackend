using Casino.Backend.Enums;
using Casino.Backend.Models;

namespace Casino.Backend.Services.Interfaces
{
  /// <summary>
    /// Service for processing payments (deposits and withdrawals)
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Create a payment intent for deposit
        /// </summary>
      Task<PaymentResult> CreateDepositAsync(int userId, decimal amount, PaymentProvider provider, PaymentMethod method, string? ipAddress = null);
        
  /// <summary>
        /// Process a completed deposit (called by webhook)
        /// </summary>
        Task<PaymentResult> ProcessDepositAsync(string externalTransactionId, PaymentStatus status);
 
 /// <summary>
        /// Create a withdrawal request
    /// </summary>
        Task<PaymentResult> CreateWithdrawalAsync(int userId, decimal amount, PaymentMethod method, string? accountDetails = null);
      
      /// <summary>
        /// Process withdrawal (admin approval required)
        /// </summary>
        Task<PaymentResult> ProcessWithdrawalAsync(int paymentId, bool approved, string? adminNotes = null);
  
  /// <summary>
      /// Get payment by ID
      /// </summary>
 Task<Payment?> GetPaymentByIdAsync(int paymentId);
   
      /// <summary>
   /// Get payment by external transaction ID
        /// </summary>
   Task<Payment?> GetPaymentByExternalIdAsync(string externalTransactionId);
  
        /// <summary>
    /// Get user's payment history
   /// </summary>
  Task<IEnumerable<Payment>> GetUserPaymentsAsync(int userId, PaymentType? type = null);
        
 /// <summary>
        /// Cancel a pending payment
    /// </summary>
    Task<PaymentResult> CancelPaymentAsync(int paymentId, int userId);
  
  /// <summary>
        /// Refund a completed payment
 /// </summary>
        Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal? amount = null, string? reason = null);
    }

    /// <summary>
    /// Result of a payment operation
    /// </summary>
    public class PaymentResult
    {
   public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
   public int? PaymentId { get; set; }
        public string? ClientSecret { get; set; }  // For Stripe payment intent
public string? PaymentUrl { get; set; }     // For PayPal, etc.
    public PaymentStatus Status { get; set; }
  public List<string> Errors { get; set; } = new();
        public object? Data { get; set; }

        public static PaymentResult Successful(int paymentId, PaymentStatus status, string message = "Payment processed successfully")
 {
            return new PaymentResult
        {
  Success = true,
          Message = message,
   PaymentId = paymentId,
                Status = status
    };
        }

     public static PaymentResult RequiresAction(int paymentId, string clientSecret, string paymentUrl)
        {
            return new PaymentResult
  {
    Success = true,
          Message = "Payment requires additional action",
     PaymentId = paymentId,
                ClientSecret = clientSecret,
 PaymentUrl = paymentUrl,
      Status = PaymentStatus.RequiresAction
    };
        }

        public static PaymentResult Error(string errorMessage)
        {
    return new PaymentResult
 {
   Success = false,
     Message = errorMessage,
 Status = PaymentStatus.Failed,
       Errors = new List<string> { errorMessage }
  };
  }

   public static PaymentResult InsufficientFunds()
   {
         return new PaymentResult
{
  Success = false,
   Message = "Insufficient funds for withdrawal",
    Status = PaymentStatus.Failed,
Errors = new List<string> { "Your balance is too low for this withdrawal." }
 };
 }
    }
}
