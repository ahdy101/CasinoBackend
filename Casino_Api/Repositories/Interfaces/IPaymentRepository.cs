using Casino.Backend.Enums;
using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
  public interface IPaymentRepository : IRepository<Payment>
    {
   Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId);
  Task<Payment?> GetByReferenceNumberAsync(string referenceNumber);
        Task<IEnumerable<Payment>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Payment>> GetByUserIdAndTypeAsync(int userId, PaymentType type);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
        Task<IEnumerable<Payment>> GetPendingWithdrawalsAsync();
        Task<bool> UpdateStatusAsync(int paymentId, PaymentStatus status, string? errorMessage = null);
    }
}
