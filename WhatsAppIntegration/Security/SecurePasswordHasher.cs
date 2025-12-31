using System;
using System.Security.Cryptography;

namespace UstadDesktop.WhatsAppIntegration.Security
{
    /// <summary>
    /// Secure password hashing using PBKDF2 with SHA256
    /// Implements industry best practices for password security
    /// </summary>
    public static class SecurePasswordHasher
    {
        // Security parameters - adjust based on security requirements and performance
        private const int SaltSize = 32; // 32 bytes = 256 bits
        private const int HashSize = 32; // 32 bytes = 256 bits  
        private const int DefaultIterations = 200_000; // 200k iterations (adjust for performance)

        /// <summary>
        /// Hash a password using PBKDF2 with random salt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="iterations">Optional custom iteration count</param>
        /// <returns>Tuple containing hash, salt, and iteration count</returns>
        public static (byte[] Hash, byte[] Salt, int Iterations) HashPassword(string password, int? iterations = null)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            int iterationCount = iterations ?? DefaultIterations;
            
            // Generate random salt
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            // Generate hash using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterationCount, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            return (hash, salt, iterationCount);
        }

        /// <summary>
        /// Verify a password against stored hash, salt, and iterations
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="storedHash">Stored password hash</param>
        /// <param name="storedSalt">Stored salt</param>
        /// <param name="iterations">Stored iteration count</param>
        /// <returns>True if password matches</returns>
        public static bool Verify(string password, byte[] storedHash, byte[] storedSalt, int iterations)
        {
            if (string.IsNullOrEmpty(password))
                return false;
            
            if (storedHash == null || storedSalt == null)
                return false;

            // Generate hash with same parameters
            using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, iterations, HashAlgorithmName.SHA256);
            byte[] computed = pbkdf2.GetBytes(storedHash.Length);
            
            // Use timing-safe comparison
            return FixedTimeEquals(storedHash, computed);
        }

        /// <summary>
        /// Timing-safe byte array comparison to prevent timing attacks
        /// </summary>
        /// <param name="a">First byte array</param>
        /// <param name="b">Second byte array</param>
        /// <returns>True if arrays are equal</returns>
        private static bool FixedTimeEquals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            if (a.Length != b.Length) 
                return false;
            
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            
            return diff == 0;
        }

        /// <summary>
        /// Generate a cryptographically secure random token for password resets
        /// </summary>
        /// <param name="length">Token length in bytes (default 32)</param>
        /// <returns>Base64 encoded token</returns>
        public static string GenerateSecureToken(int length = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] tokenBytes = new byte[length];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");
        }

        /// <summary>
        /// Validate password strength (basic implementation)
        /// Extend this method based on your security requirements
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password meets minimum requirements</returns>
        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsLetterOrDigit(c)) hasSpecial = true;
            }

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        /// <summary>
        /// Generate a random password meeting security requirements
        /// </summary>
        /// <param name="length">Password length (minimum 12)</param>
        /// <returns>Secure random password</returns>
        public static string GenerateSecurePassword(int length = 16)
        {
            if (length < 12)
                throw new ArgumentException("Password length must be at least 12 characters", nameof(length));

            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            const string allChars = uppercase + lowercase + digits + special;

            using var rng = RandomNumberGenerator.Create();
            var password = new char[length];

            // Ensure at least one character from each category
            password[0] = uppercase[GetRandomIndex(rng, uppercase.Length)];
            password[1] = lowercase[GetRandomIndex(rng, lowercase.Length)];
            password[2] = digits[GetRandomIndex(rng, digits.Length)];
            password[3] = special[GetRandomIndex(rng, special.Length)];

            // Fill remaining positions with random characters
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[GetRandomIndex(rng, allChars.Length)];
            }

            // Shuffle the password to avoid predictable patterns
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = GetRandomIndex(rng, i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }

        private static int GetRandomIndex(RandomNumberGenerator rng, int maxValue)
        {
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            return Math.Abs(BitConverter.ToInt32(bytes, 0)) % maxValue;
        }
    }
}
