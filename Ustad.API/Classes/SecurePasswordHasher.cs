using System;
using System.Security.Cryptography;

namespace Ustad.API.Classes
{
    public static class SecurePasswordHasher
    {
        private const int SaltSize = 32; // 256-bit
        private const int HashSize = 32; // 256-bit
        private const int DefaultIterations = 200_000;

        public static (byte[] Hash, byte[] Salt, int Iterations) HashPassword(string password, int? iterations = null)
        {
            int it = iterations ?? DefaultIterations;
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, it, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize);
                    return (hash, salt, it);
                }
            }
        }

        public static bool Verify(string password, byte[] storedHash, byte[] storedSalt, int iterations)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] computed = pbkdf2.GetBytes(storedHash.Length);
                return FixedTimeEquals(storedHash, computed);
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}


