using System;
using System.Security.Cryptography;

namespace Isolaatti.Utils
{
    public static class RandomData
    {
        public static string GenerateRandomKey(int length)
        {
            var randomBytes = new byte[length];
            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public static string GenerateRandomPassword()
        {
            var randomData = new byte[10];
            RandomNumberGenerator.Create().GetBytes(randomData);
            return Convert.ToBase64String(randomData);
        }
    }
}