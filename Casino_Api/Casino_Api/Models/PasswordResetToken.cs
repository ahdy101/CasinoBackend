namespace Casino.Backend.Models
{
    /// <summary>
    /// Password reset tokens for secure password recovery
    /// </summary>
    public class PasswordResetToken
    {
        public int Id { get; set; }
   
        /// <summary>
        /// User ID this token belongs to
        /// </summary>
   public int UserId { get; set; }
 
        /// <summary>
 /// Unique reset token (generated securely)
        /// </summary>
        public string Token { get; set; } = string.Empty;
     
        /// <summary>
        /// When the token expires (typically 1 hour)
        /// </summary>
   public DateTime ExpiresAt { get; set; }
 
        /// <summary>
        /// Whether token has been used
      /// </summary>
        public bool IsUsed { get; set; } = false;
        
        /// <summary>
        /// When token was created
        /// </summary>
     public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
 /// <summary>
        /// When token was used
 /// </summary>
      public DateTime? UsedAt { get; set; }
        
        // Navigation property
        public User? User { get; set; }
    }
}
