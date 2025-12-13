using System.Security.Cryptography;

namespace Casino.Backend.Infrastructure
{
    /// <summary>
    /// Interface for cryptographically secure random number generation
    /// </summary>
    public interface IRandomNumberGenerator
    {
        /// <summary>
        /// Generate a cryptographically secure random integer between min (inclusive) and max (exclusive)
        /// </summary>
        int GetRandomInt(int min, int max);

   /// <summary>
        /// Generate a cryptographically secure random double between 0.0 and 1.0
  /// </summary>
        double GetRandomDouble();

      /// <summary>
      /// Fill a byte array with cryptographically secure random bytes
        /// </summary>
void FillBytes(byte[] buffer);
    }

    /// <summary>
    /// Cryptographically secure random number generator implementation
    /// ?? CRITICAL: Always use this for casino games, NEVER use System.Random
    /// </summary>
    public class CryptoRNG : IRandomNumberGenerator
    {
        /// <summary>
        /// Generate a cryptographically secure random integer between min (inclusive) and max (exclusive)
        /// </summary>
        public int GetRandomInt(int min, int max)
  {
     if (min >= max)
     throw new ArgumentException("min must be less than max");

    var bytes = new byte[4];
      RandomNumberGenerator.Fill(bytes);
            int value = Math.Abs(BitConverter.ToInt32(bytes, 0));
        return min + (value % (max - min));
        }

  /// <summary>
    /// Generate a cryptographically secure random double between 0.0 and 1.0
  /// </summary>
        public double GetRandomDouble()
        {
   var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
       uint value = BitConverter.ToUInt32(bytes, 0);
        return value / (double)uint.MaxValue;
        }

        /// <summary>
        /// Fill a byte array with cryptographically secure random bytes
        /// </summary>
        public void FillBytes(byte[] buffer)
     {
        RandomNumberGenerator.Fill(buffer);
        }
 }
}
