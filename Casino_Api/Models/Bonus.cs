namespace Casino.Backend.Models
{
    /// <summary>
    /// Bonus/promotion configuration
    /// </summary>
    public class Bonus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }  // Promo code (optional)
        public BonusType Type { get; set; }
        public decimal? Amount { get; set; }  // Fixed amount
        public decimal? Percentage { get; set; }  // e.g., 100% match
  public decimal? MaxBonus { get; set; }  // Cap on bonus
        public decimal? MinDeposit { get; set; }  // Minimum deposit required
        public decimal WageringMultiplier { get; set; } = 35;  // e.g., 35x wagering
        public int? FreeSpinsCount { get; set; }
        public string? FreeSpinsGameId { get; set; }
        public DateTime ValidFrom { get; set; }
public DateTime ValidTo { get; set; }
        public int? MaxClaims { get; set; }  // Total claims allowed
        public int CurrentClaims { get; set; } = 0;
    public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// User's claimed bonus
    /// </summary>
    public class UserBonus
    {
        public int Id { get; set; }
     public int UserId { get; set; }
     public int BonusId { get; set; }
    public decimal BonusAmount { get; set; }
   public decimal WageringRequired { get; set; }
  public decimal WageringCompleted { get; set; } = 0;
        public UserBonusStatus Status { get; set; } = UserBonusStatus.Active;
        public DateTime ExpiresAt { get; set; }
        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public User? User { get; set; }
 public Bonus? Bonus { get; set; }
    }

    public enum BonusType
    {
        Welcome = 1,
        DepositMatch = 2,
        FreeSpins = 3,
        Cashback = 4,
        Reload = 5,
        VIP = 6,
        NoDeposit = 7
    }

    public enum UserBonusStatus
    {
        Active = 1,
        Completed = 2,
   Expired = 3,
        Cancelled = 4,
        Forfeited = 5
    }
}
