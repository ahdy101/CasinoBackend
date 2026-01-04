using Casino.Backend.Enums;

namespace Casino.Backend.Models
{
    /// <summary>
   /// Sensitive KYC details stored separately with encryption
/// </summary>
    public class KycDetails
    {
        public int Id { get; set; }
     public int UserId { get; set; }
     public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string GovernmentIdNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
  public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
     public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    public KycStatus Status { get; set; } = KycStatus.NotStarted;
   public string? RejectionReason { get; set; }
        public int? ReviewedByAdminId { get; set; }
        public DateTime? SubmittedAt { get; set; }
     public DateTime? VerifiedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? ModifiedAt { get; set; }

  public User? User { get; set; }
  }
}
