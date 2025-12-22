namespace Casino.Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; }
        public decimal Balance { get; set; } = 0m;
        public string Role { get; set; } = "Player";
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? TenantId { get; set; }
    }

}
