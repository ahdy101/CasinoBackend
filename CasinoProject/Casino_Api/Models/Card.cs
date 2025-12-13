namespace Casino_Api.Models;

public class Card
{
    public string Suit { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    
    public int GetNumericValue()
    {
        return Value switch
        {
            "A" => 11,
            "K" or "Q" or "J" => 10,
            _ => int.TryParse(Value, out int val) ? val : 0
        };
    }
    
    public bool IsAce() => Value == "A";
}
