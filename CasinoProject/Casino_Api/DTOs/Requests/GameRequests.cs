using System.ComponentModel.DataAnnotations;

namespace Casino_Api.DTOs.Requests;

public class SpinSlotsRequest
{
    [Required]
    [Range(1, 10000)]
    public decimal BetAmount { get; set; }
}

public class AddFundsRequest
{
    [Required]
    [Range(1, 100000)]
    public decimal Amount { get; set; }
}

public class CashOutRequest
{
    [Required]
    [Range(1, 100000)]
    public decimal Amount { get; set; }
}
