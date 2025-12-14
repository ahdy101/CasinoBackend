namespace Casino_Api.DTOs.Responses;

public class GameStateResponse
{
    public int Id { get; set; }
    public string GameType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GameStatsResponse
{
    public string GameType { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Pushes { get; set; }
    public decimal TotalWagered { get; set; }
    public decimal TotalWon { get; set; }
    public decimal WinRate { get; set; }
    public decimal Profit { get; set; }
}

public class AllGameStatsResponse
{
    public List<GameStatsResponse> Stats { get; set; } = new();
}
