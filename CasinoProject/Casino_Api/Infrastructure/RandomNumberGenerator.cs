using System.Security.Cryptography;

namespace Casino_Api.Infrastructure;

public interface IRandomNumberGenerator
{
    int Next(int min, int max);
}

public class CryptoRNG : IRandomNumberGenerator
{
    public int Next(int min, int max)
    {
        if (min >= max)
            throw new ArgumentException("min must be less than max");

        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        int val = Math.Abs(BitConverter.ToInt32(bytes, 0));
        return min + (val % (max - min));
    }
}
