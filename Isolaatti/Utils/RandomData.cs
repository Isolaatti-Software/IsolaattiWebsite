using System;
using System.Security.Cryptography;
using System.Text;

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
        
        private const string AllowedCharactersForUniqueCode =
            "ABCDEFGHIJKLMNOPKRSTUVWXYZabcdefghijklmnopkrstuvwxyz1234567890";

        public static string GenerateRandomString(int lenght)
        {
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < lenght; i++)
            {
                stringBuilder.Append(
                    AllowedCharactersForUniqueCode[
                        RandomNumberGenerator.GetInt32(0, AllowedCharactersForUniqueCode.Length)]);
            }

            return stringBuilder.ToString();
        }
    }
}