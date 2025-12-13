using System.ComponentModel.DataAnnotations;

namespace Casino_Api.DTOs.Requests;

public class RouletteBetRequest
{
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // "straight", "color", "evenOdd", etc.
    
    [Required]
    public string Position { get; set; } = string.Empty; // number or "red", "black", "even", "odd"
    
    [Required]
    [Range(1, 10000)]
    public decimal Amount { get; set; }
}

public class SpinRouletteRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one bet is required")]
    public List<RouletteBetRequest> Bets { get; set; } = new();
}
