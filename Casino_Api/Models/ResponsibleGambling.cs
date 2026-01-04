namespace Casino.Backend.Models
{
    /// <summary>
    /// User self-imposed limits for responsible gambling
    /// </summary>
    public class UserLimit
    {
        public int Id { get; set; }
     public int UserId { get; set; }
     public LimitType LimitType { get; set; }
        public decimal LimitAmount { get; set; }
public decimal CurrentUsage { get; set; } = 0;
  public DateTime PeriodStart { get; set; }
      public DateTime PeriodEnd { get; set; }
        public bool IsActive { get; set; } = true;
    public DateTime? CooldownUntil { get; set; }  // For cooling-off periods
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }

        // Navigation
        public User? User { get; set; }
    }

    /// <summary>
    /// Self-exclusion records
    /// </summary>
    public class UserExclusion
  {
        public int Id { get; set; }
 public int UserId { get; set; }
 public ExclusionType Type { get; set; }
   public ExclusionReason Reason { get; set; }
        public DateTime StartDate { get; set; }
   public DateTime? EndDate { get; set; }  // Null = permanent
        public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
        public int? RevokedByAdminId { get; set; }

        // Navigation
  public User? User { get; set; }
    }

    public enum LimitType
    {
        DailyDeposit = 1,
        WeeklyDeposit = 2,
     MonthlyDeposit = 3,
        DailyLoss = 4,
        WeeklyLoss = 5,
        MonthlyLoss = 6,
  DailyWager = 7,
        SessionTime = 8  // In minutes
    }

    public enum ExclusionType
 {
        Temporary = 1,    // 24 hours to 6 months
        LongTerm = 2,     // 6 months to 5 years
        Permanent = 3     // Lifetime
    }

    public enum ExclusionReason
    {
        SelfRequested = 1,
        ProblemGambling = 2,
        FinancialConcerns = 3,
        TakeABreak = 4,
        Other = 5
  }
}
