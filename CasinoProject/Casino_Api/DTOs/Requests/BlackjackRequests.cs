using System.ComponentModel.DataAnnotations;

namespace Casino_Api.DTOs.Requests;

public class DealRequest
{
    [Required]
    [Range(1, 100000, ErrorMessage = "Bet amount must be between 1 and 100,000")]
    public decimal BetAmount { get; set; }
}

public class HitRequest
{
    [Required]
    public int GameId { get; set; }
}

public class StandRequest
{
    [Required]
    public int GameId { get; set; }
}

public class DoubleDownRequest
{
    [Required]
    public int GameId { get; set; }
}

public class SplitRequest
{
    [Required]
    public int GameId { get; set; }
}
