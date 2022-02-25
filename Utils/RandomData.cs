using System;
using System.Security.Cryptography;

namespace isolaatti_API.Utils
{
    public class RandomData
    {
        public static string GenerateRandomKey(int length)
        {
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[length];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}