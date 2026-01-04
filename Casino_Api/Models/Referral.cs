namespace Casino.Backend.Models
{
    /// <summary>
    /// User referral tracking
    /// </summary>
 public class Referral
    {
        public int Id { get; set; }
        public int ReferrerId { get; set; }  // User who referred
        public int ReferredUserId { get; set; }// New user who signed up
        public string ReferralCode { get; set; } = string.Empty;
  public ReferralStatus Status { get; set; } = ReferralStatus.Pending;
  public bool QualifyingDepositMade { get; set; } = false;
public decimal? QualifyingDepositAmount { get; set; }
     public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? QualifiedAt { get; set; }

  // Navigation
      public User? Referrer { get; set; }
        public User? ReferredUser { get; set; }
    }

    /// <summary>
    /// Referral reward payouts
    /// </summary>
    public class ReferralReward
    {
        public int Id { get; set; }
        public int ReferralId { get; set; }
   public int UserId { get; set; }  // Who receives the reward
        public RewardType RewardType { get; set; }
    public decimal Amount { get; set; }
        public string? BonusCode { get; set; }
        public bool IsPaid { get; set; } = false;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // Navigation
     public Referral? Referral { get; set; }
     public User? User { get; set; }
    }

    public enum ReferralStatus
    {
        Pending = 1,      // Signed up but no deposit
        Qualified = 2,    // Made qualifying deposit
   Rewarded = 3,     // Rewards paid out
        Expired = 4       // Didn't qualify in time
    }

    public enum RewardType
  {
        ReferrerCash = 1,
ReferrerBonus = 2,
        ReferredBonus = 3,
        ReferredFreeSpins = 4
    }
}
