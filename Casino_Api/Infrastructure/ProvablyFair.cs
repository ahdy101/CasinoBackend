using System.Security.Cryptography;
using System.Text;

namespace Casino.Backend.Models
{
    /// <summary>
    /// Server seeds for provably fair gaming
    /// </summary>
    public class ProvablyFairSeed
    {
     public int Id { get; set; }
 public int UserId { get; set; }
  public string ServerSeed { get; set; } = string.Empty;
public string ServerSeedHash { get; set; } = string.Empty;  // SHA256 hash shown to user
  public string? ClientSeed { get; set; }
        public int Nonce { get; set; } = 0;  // Increments per bet
        public bool IsActive { get; set; } = true;
        public bool IsRevealed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? RevealedAt { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}

namespace Casino.Backend.Infrastructure
{
    /// <summary>
    /// Provably fair game verification
    /// </summary>
    public static class ProvablyFairHelper
    {
        /// <summary>
 /// Generate a new server seed
      /// </summary>
        public static (string seed, string hash) GenerateServerSeed()
        {
         var bytes = RandomNumberGenerator.GetBytes(32);
         var seed = Convert.ToHexString(bytes).ToLowerInvariant();
  var hash = ComputeSha256Hash(seed);
         return (seed, hash);
 }

        /// <summary>
        /// Compute SHA256 hash
        /// </summary>
public static string ComputeSha256Hash(string input)
        {
       var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
 return Convert.ToHexString(bytes).ToLowerInvariant();
  }

        /// <summary>
        /// Generate game result using HMAC
/// </summary>
        public static string GenerateGameResult(string serverSeed, string clientSeed, int nonce)
  {
        var message = $"{clientSeed}:{nonce}";
      var key = Encoding.UTF8.GetBytes(serverSeed);
    var messageBytes = Encoding.UTF8.GetBytes(message);
         
   using var hmac = new HMACSHA256(key);
       var hash = hmac.ComputeHash(messageBytes);
       return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>
   /// Convert hash to a number in range [0, max)
        /// Used for determining game outcomes
        /// </summary>
        public static int HashToNumber(string hash, int max)
        {
       // Take first 8 characters (32 bits)
    var hex = hash[..8];
       var value = Convert.ToUInt32(hex, 16);
        return (int)(value % max);
    }

        /// <summary>
        /// Verify a game result
        /// </summary>
        public static bool VerifyResult(string serverSeed, string clientSeed, int nonce, string expectedHash)
        {
     var computedHash = GenerateGameResult(serverSeed, clientSeed, nonce);
            return computedHash == expectedHash;
        }
    }
}
