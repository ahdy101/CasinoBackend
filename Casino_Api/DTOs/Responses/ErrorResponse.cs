namespace Casino.Backend.DTOs.Responses
{
    /// <summary>
    /// Standard error response format for all API errors
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Indicates whether the operation was successful (always false for ErrorResponse)
      /// </summary>
        public bool Success { get; set; } = false;

     /// <summary>
   /// List of error messages
    /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
  /// Main error message
        /// </summary>
public string Message { get; set; } = string.Empty;

        /// <summary>
   /// Timestamp when the error occurred
    /// </summary>
      public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
