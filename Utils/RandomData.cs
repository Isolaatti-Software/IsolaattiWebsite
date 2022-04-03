using System;
using System.Security.Cryptography;

namespace isolaatti_API.Utils
{
    public static class RandomData
    {
        public static string GenerateRandomKey(int length)
        {
            var randomBytes = new byte[length];
            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}