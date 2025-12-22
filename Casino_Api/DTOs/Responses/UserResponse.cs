namespace Casino.Backend.DTOs.Responses
{
    /// <summary>
    /// Response model for user data (without sensitive information)
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Current wallet balance
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// When the account was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
