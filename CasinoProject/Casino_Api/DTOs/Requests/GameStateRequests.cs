namespace Casino_Api.DTOs.Requests;

public class SaveGameStateRequest
{
    public string GameType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public class UpdateGameStatsRequest
{
    public string GameType { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty; // "win", "loss", "push"
    public decimal Wagered { get; set; }
    public decimal Won { get; set; }
}
