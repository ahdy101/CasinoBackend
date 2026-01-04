using System.Security.Cryptography;
using System.Text;

namespace Casino.Backend.Infrastructure
{
    /// <summary>
    /// Helper for ETag generation and validation (optimistic concurrency control)
    /// </summary>
 public static class ETagHelper
    {
/// <summary>
        /// Generate ETag from entity properties
     /// </summary>
     /// <param name="entityId">Entity ID</param>
        /// <param name="modifiedAt">Last modified timestamp</param>
    /// <returns>Base64-encoded SHA256 hash</returns>
        public static string GenerateETag(int entityId, DateTime? modifiedAt)
        {
            var timestamp = modifiedAt?.ToString("O") ?? DateTime.UtcNow.ToString("O");
          var input = $"{entityId}{timestamp}";
      var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
       return Convert.ToBase64String(hashBytes);
        }

      /// <summary>
        /// Generate ETag from any object (uses JSON serialization)
     /// </summary>
 public static string GenerateETag(object entity)
      {
   var json = System.Text.Json.JsonSerializer.Serialize(entity);
var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(hashBytes);
        }

 /// <summary>
   /// Validate If-Match header against current ETag
        /// </summary>
  /// <param name="ifMatchHeader">If-Match header value from request</param>
   /// <param name="currentETag">Current entity ETag</param>
      /// <returns>True if ETags match</returns>
     public static bool ValidateETag(string? ifMatchHeader, string currentETag)
        {
       if (string.IsNullOrWhiteSpace(ifMatchHeader))
      return false;

    // Remove quotes if present (some clients send "etag" instead of etag)
         ifMatchHeader = ifMatchHeader.Trim('"');

            return ifMatchHeader.Equals(currentETag, StringComparison.Ordinal);
     }
    }
}
