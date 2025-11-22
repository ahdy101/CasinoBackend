namespace Casino_Admin.Models
{
    public class Bet
    {
   public int Id { get; set; }
  public int UserId { get; set; }
   public User User { get; set; }
    public string Game { get; set; }
    public decimal Amount { get; set; }
    public string Choice { get; set; }
        public decimal Payout { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}