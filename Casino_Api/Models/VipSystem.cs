namespace Casino.Backend.Models
{
    /// <summary>
    /// VIP tier definitions
    /// </summary>
    public class VipLevel
    {
     public int Id { get; set; }
public string Name { get; set; } = string.Empty;  // Bronze, Silver, Gold, Platinum, Diamond
        public int Level { get; set; }  // 1, 2, 3, 4, 5
        public int PointsRequired { get; set; }
        public decimal CashbackPercentage { get; set; }
        public decimal DepositBonusPercentage { get; set; }
     public int FreeSpinsPerMonth { get; set; }
   public bool HasDedicatedManager { get; set; }
        public bool HasFasterWithdrawals { get; set; }
    public decimal WithdrawalLimitMultiplier { get; set; } = 1.0m;
      public string? Benefits { get; set; }  // JSON array of benefits
    public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// User's VIP status and points
    /// </summary>
    public class UserVipStatus
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VipLevelId { get; set; }
        public int TotalPoints { get; set; } = 0;
        public int CurrentPeriodPoints { get; set; } = 0;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    public DateTime? LevelUpDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }

        // Navigation
   public User? User { get; set; }
        public VipLevel? VipLevel { get; set; }
    }

    /// <summary>
    /// Loyalty points ledger
    /// </summary>
    public class LoyaltyPointTransaction
    {
      public int Id { get; set; }
        public int UserId { get; set; }
        public int Points { get; set; }// Positive = earned, Negative = redeemed
        public LoyaltyPointType Type { get; set; }
   public string Description { get; set; } = string.Empty;
        public int? RelatedBetId { get; set; }
        public int BalanceBefore { get; set; }
      public int BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
    }

    public enum LoyaltyPointType
    {
        Earned = 1,      // From wagering
        Redeemed = 2,    // Converted to cash/bonus
   Expired = 3,     // Monthly expiry
        Bonus = 4,       // Promotional points
      Adjustment = 5   // Admin adjustment
    }
}
