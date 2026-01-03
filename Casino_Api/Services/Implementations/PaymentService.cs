using Casino.Backend.Enums;
using Casino.Backend.Models;
using Casino.Backend.Repositories.Interfaces;
using Casino.Backend.Services.Interfaces;
// using Stripe;  // Commented out temporarily
// using PaymentMethod = Casino.Backend.Enums.PaymentMethod;

namespace Casino.Backend.Services.Implementations
{
    /// <summary>
    /// Payment processing service (Simplified version without Stripe for now)
    /// </summary>
public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
 private readonly IWalletService _walletService;
      private readonly IWalletTransactionRepository _walletTransactionRepository;
  private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;

 public PaymentService(
            IPaymentRepository paymentRepository,
IWalletService walletService,
      IWalletTransactionRepository walletTransactionRepository,
    IConfiguration configuration,
    ILogger<PaymentService> logger)
   {
   _paymentRepository = paymentRepository;
_walletService = walletService;
 _walletTransactionRepository = walletTransactionRepository;
   _configuration = configuration;
 _logger = logger;
      }

     public async Task<PaymentResult> CreateDepositAsync(
   int userId, 
   decimal amount, 
            PaymentProvider provider, 
    PaymentMethod method, 
      string? ipAddress = null)
        {
  try
    {
   _logger.LogInformation("Creating deposit for user {UserId}, amount {Amount}", userId, amount);

         // Validate amount
 if (amount <= 0)
      {
      return PaymentResult.Error("Amount must be greater than zero");
  }

         if (amount < 10)
   {
 return PaymentResult.Error("Minimum deposit is $10");
  }

            if (amount > 10000)
      {
                return PaymentResult.Error("Maximum deposit is $10,000 per transaction");
    }

  // Generate unique reference number
    var refNumber = $"DEP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

    // Calculate processing fees
 var fee = (amount * 0.029m) + 0.30m;
       var netAmount = amount - fee;

// Create payment record
     var payment = new Payment
{
            UserId = userId,
    Type = PaymentType.Deposit,
        Amount = amount,
             Provider = provider,
    Method = method,
Status = PaymentStatus.Pending,
    ReferenceNumber = refNumber,
 Currency = "USD",
     Fee = fee,
 NetAmount = netAmount,
        IpAddress = ipAddress,
  CreatedAt = DateTime.UtcNow
    };

      await _paymentRepository.AddAsync(payment);

      // For now, auto-approve all deposits (demo mode)
   return await AutoApproveDepositAsync(payment);
   }
       catch (Exception ex)
      {
  _logger.LogError(ex, "Error creating deposit for user {UserId}", userId);
    return PaymentResult.Error($"Error processing payment: {ex.Message}");
}
        }

        private async Task<PaymentResult> AutoApproveDepositAsync(Payment payment)
      {
     try
     {
payment.Status = PaymentStatus.Completed;
    payment.CompletedAt = DateTime.UtcNow;
    payment.UpdatedAt = DateTime.UtcNow;
             await _paymentRepository.UpdateAsync(payment);

var walletResult = await _walletService.AddFunds(payment.UserId, payment.NetAmount);
    
           if (walletResult.Success)
    {
     var walletTransactions = await _walletTransactionRepository.GetByUserIdAsync(payment.UserId);
       var latestTransaction = walletTransactions.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
            
    if (latestTransaction != null)
              {
  payment.WalletTransactionId = latestTransaction.Id;
      await _paymentRepository.UpdateAsync(payment);
   }
       }

_logger.LogInformation("Deposit auto-approved for payment {PaymentId}", payment.Id);

           return PaymentResult.Successful(
        payment.Id, 
       PaymentStatus.Completed,
   "Deposit successful (Demo mode)");
   }
            catch (Exception ex)
   {
       _logger.LogError(ex, "Error auto-approving deposit");
                return PaymentResult.Error($"Error processing deposit: {ex.Message}");
}
}

  public async Task<PaymentResult> ProcessDepositAsync(string externalTransactionId, PaymentStatus status)
     {
            try
         {
     _logger.LogInformation("Processing deposit for transaction {ExternalId}, status {Status}", 
 externalTransactionId, status);

                var payment = await _paymentRepository.GetByExternalTransactionIdAsync(externalTransactionId);
     if (payment == null)
  {
        _logger.LogWarning("Payment not found for external transaction {ExternalId}", externalTransactionId);
  return PaymentResult.Error("Payment not found");
 }

if (payment.Status == PaymentStatus.Completed)
                {
  _logger.LogInformation("Payment {PaymentId} already completed", payment.Id);
      return PaymentResult.Successful(payment.Id, PaymentStatus.Completed, "Payment already processed");
              }

payment.Status = status;
payment.UpdatedAt = DateTime.UtcNow;
   
       if (status == PaymentStatus.Completed)
    {
   payment.CompletedAt = DateTime.UtcNow;
  
       var walletResult = await _walletService.AddFunds(payment.UserId, payment.NetAmount);
 
    if (walletResult.Success)
    {
      var walletTransactions = await _walletTransactionRepository.GetByUserIdAsync(payment.UserId);
            var latestTransaction = walletTransactions.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
    
  if (latestTransaction != null)
         {
  payment.WalletTransactionId = latestTransaction.Id;
  }
    
_logger.LogInformation("Funds added to wallet for payment {PaymentId}", payment.Id);
       }
    else
         {
    _logger.LogError("Failed to add funds to wallet for payment {PaymentId}: {Error}", 
   payment.Id, walletResult.Message);
    payment.Status = PaymentStatus.Failed;
   payment.ErrorMessage = walletResult.Message;
       }
      }
       else if (status == PaymentStatus.Failed)
    {
       payment.CompletedAt = DateTime.UtcNow;
    }

await _paymentRepository.UpdateAsync(payment);

      return PaymentResult.Successful(payment.Id, status);
}
     catch (Exception ex)
      {
            _logger.LogError(ex, "Error processing deposit for transaction {ExternalId}", externalTransactionId);
   return PaymentResult.Error($"Error processing deposit: {ex.Message}");
        }
        }

        public async Task<PaymentResult> CreateWithdrawalAsync(
         int userId, 
            decimal amount, 
         PaymentMethod method, 
            string? accountDetails = null)
      {
     try
    {
      _logger.LogInformation("Creating withdrawal for user {UserId}, amount {Amount}", userId, amount);

            if (amount <= 0)
     {
        return PaymentResult.Error("Amount must be greater than zero");
    }

        if (amount < 20)
            {
 return PaymentResult.Error("Minimum withdrawal is $20");
    }

   var balance = await _walletService.GetBalance(userId);
   if (balance < amount)
   {
     return PaymentResult.InsufficientFunds();
         }

var refNumber = $"WTH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    var fee = 2.00m;
         var netAmount = amount - fee;

    var payment = new Payment
      {
           UserId = userId,
     Type = PaymentType.Withdrawal,
  Amount = amount,
     Provider = PaymentProvider.Manual,
    Method = method,
       Status = PaymentStatus.Pending,
 ReferenceNumber = refNumber,
   Currency = "USD",
     Fee = fee,
    NetAmount = netAmount,
        Metadata = accountDetails,
    CreatedAt = DateTime.UtcNow
};

     await _paymentRepository.AddAsync(payment);

           var walletResult = await _walletService.DeductBet(userId, amount, "Withdrawal", refNumber);

   if (walletResult.Success && walletResult.BetId.HasValue)
      {
var walletTransactions = await _walletTransactionRepository.GetByUserIdAsync(userId);
   var latestTransaction = walletTransactions.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
 
   if (latestTransaction != null)
      {
             payment.WalletTransactionId = latestTransaction.Id;
        await _paymentRepository.UpdateAsync(payment);
     }
             }

     _logger.LogInformation("Withdrawal request created: {PaymentId}", payment.Id);

     return PaymentResult.Successful(
       payment.Id, 
PaymentStatus.Pending, 
   "Withdrawal request submitted. Pending admin approval.");
       }
      catch (Exception ex)
{
_logger.LogError(ex, "Error creating withdrawal for user {UserId}", userId);
            return PaymentResult.Error($"Error creating withdrawal: {ex.Message}");
 }
        }

   public async Task<PaymentResult> ProcessWithdrawalAsync(int paymentId, bool approved, string? adminNotes = null)
        {
       try
    {
    var payment = await _paymentRepository.GetByIdAsync(paymentId);
   if (payment == null)
     {
    return PaymentResult.Error("Payment not found");
       }

     if (payment.Type != PaymentType.Withdrawal)
    {
  return PaymentResult.Error("Not a withdrawal payment");
         }

if (payment.Status != PaymentStatus.Pending)
       {
   return PaymentResult.Error($"Withdrawal already processed (Status: {payment.Status})");
        }

    if (approved)
       {
     payment.Status = PaymentStatus.Completed;
    payment.CompletedAt = DateTime.UtcNow;
_logger.LogInformation("Withdrawal {PaymentId} approved", paymentId);
        }
     else
{
   payment.Status = PaymentStatus.Cancelled;
     payment.ErrorMessage = adminNotes ?? "Withdrawal rejected by admin";
      
      if (payment.WalletTransactionId.HasValue)
          {
     await _walletService.AddFunds(payment.UserId, payment.Amount);
   _logger.LogInformation("Withdrawal {PaymentId} rejected, funds refunded", paymentId);
 }
     }

payment.UpdatedAt = DateTime.UtcNow;
     await _paymentRepository.UpdateAsync(payment);

 return PaymentResult.Successful(payment.Id, payment.Status);
     }
       catch (Exception ex)
            {
   _logger.LogError(ex, "Error processing withdrawal {PaymentId}", paymentId);
     return PaymentResult.Error($"Error processing withdrawal: {ex.Message}");
  }
     }

public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
     {
            return await _paymentRepository.GetByIdAsync(paymentId);
        }

        public async Task<Payment?> GetPaymentByExternalIdAsync(string externalTransactionId)
{
     return await _paymentRepository.GetByExternalTransactionIdAsync(externalTransactionId);
        }

 public async Task<IEnumerable<Payment>> GetUserPaymentsAsync(int userId, PaymentType? type = null)
      {
if (type.HasValue)
    {
      return await _paymentRepository.GetByUserIdAndTypeAsync(userId, type.Value);
    }
          return await _paymentRepository.GetByUserIdAsync(userId);
        }

  public async Task<PaymentResult> CancelPaymentAsync(int paymentId, int userId)
        {
   try
    {
           var payment = await _paymentRepository.GetByIdAsync(paymentId);
        
         if (payment == null || payment.UserId != userId)
    {
  return PaymentResult.Error("Payment not found");
       }

  if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.RequiresAction)
{
     return PaymentResult.Error("Only pending payments can be cancelled");
       }

    payment.Status = PaymentStatus.Cancelled;
             payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

 return PaymentResult.Successful(payment.Id, PaymentStatus.Cancelled, "Payment cancelled successfully");
   }
    catch (Exception ex)
   {
        _logger.LogError(ex, "Error cancelling payment {PaymentId}", paymentId);
      return PaymentResult.Error($"Error cancelling payment: {ex.Message}");
      }
        }

    public async Task<PaymentResult> RefundPaymentAsync(int paymentId, decimal? amount = null, string? reason = null)
{
            try
{
          var payment = await _paymentRepository.GetByIdAsync(paymentId);
   
   if (payment == null)
       {
         return PaymentResult.Error("Payment not found");
     }

          if (payment.Status != PaymentStatus.Completed)
{
     return PaymentResult.Error("Only completed payments can be refunded");
         }

      var refundAmount = amount ?? payment.NetAmount;

payment.Status = PaymentStatus.Refunded;
    payment.ErrorMessage = reason;
   payment.UpdatedAt = DateTime.UtcNow;
  await _paymentRepository.UpdateAsync(payment);

await _walletService.AddFunds(payment.UserId, refundAmount);

       _logger.LogInformation("Payment {PaymentId} refunded, amount {Amount}", paymentId, refundAmount);

return PaymentResult.Successful(payment.Id, PaymentStatus.Refunded, "Payment refunded successfully");
 }
         catch (Exception ex)
   {
      _logger.LogError(ex, "Error refunding payment {PaymentId}", paymentId);
         return PaymentResult.Error($"Error processing refund: {ex.Message}");
 }
  }
    }
}
