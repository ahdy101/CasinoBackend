namespace Casino.Backend.Models
{
    public class Bet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Game { get; set; }           // e.g., "Roulette", "Blackjack"
        public decimal Amount { get; set; }
        public string Choice { get; set; }         // e.g., "Red", "Black", "17"
        public decimal Payout { get; set; }        // payout returned to user (0 if loss)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
