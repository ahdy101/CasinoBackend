namespace Casino.Backend.DTOs.Responses
{
    /// <summary>
    /// Response model for admin user data (excludes password hash)
    /// </summary>
    public class AdminUserResponse
    {
        public int Id { get; set; }
   public string Username { get; set; } = string.Empty;
   public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
