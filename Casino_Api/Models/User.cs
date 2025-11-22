namespace Casino.Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public decimal Balance { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
