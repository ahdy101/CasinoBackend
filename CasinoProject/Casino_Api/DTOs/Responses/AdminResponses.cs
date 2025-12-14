namespace Casino_Api.DTOs.Responses;

public record AdminDashboardResponse
{
    public UserStatistics UserStats { get; set; } = new();
    public TransactionStatistics TransactionStats { get; set; } = new();
    public GameStatistics GameStats { get; set; } = new();
}

public record UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewSignupsToday { get; set; }
    public int NewSignupsThisWeek { get; set; }
    public int NewSignupsThisMonth { get; set; }
    public List<UserActivityDto> RecentUsers { get; set; } = new();
}

public record UserActivityDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

public record TransactionStatistics
{
    public decimal TotalVolume { get; set; }
    public decimal TodayVolume { get; set; }
    public decimal WeekVolume { get; set; }
    public decimal MonthVolume { get; set; }
    public int TotalTransactions { get; set; }
    public int TodayTransactions { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
}

public record RecentTransactionDto
{
    public int BetId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? WinAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record GameStatistics
{
    public int TotalGamesPlayed { get; set; }
    public int GamesToday { get; set; }
    public int GamesThisWeek { get; set; }
    public int GamesThisMonth { get; set; }
    public decimal TotalWagered { get; set; }
    public decimal TotalWon { get; set; }
    public decimal HouseEdge { get; set; }
    public List<GameTypeStats> GameTypeStatistics { get; set; } = new();
}

public record GameTypeStats
{
    public string GameType { get; set; } = string.Empty;
    public int GamesPlayed { get; set; }
    public decimal TotalWagered { get; set; }
    public decimal TotalWon { get; set; }
    public decimal WinRate { get; set; }
}
