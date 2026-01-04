using Casino.Backend.Enums;

namespace Casino.Backend.Models
{
    /// <summary>
    /// KYC document metadata (files stored in cloud)
    /// </summary>
    public class KycDocument
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public KycStatus Status { get; set; } = KycStatus.Pending;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VerifiedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public User? User { get; set; }
    }
}
