using System;
using System.Security.Cryptography;
using System.Text;

namespace Boki_ELearning.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;
        private const string Prefix = "PBKDF2";

        public static string Hash(string rawPassword)
        {
            var input = rawPassword ?? string.Empty;
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(input, salt, Iterations, HashAlgorithmName.SHA256))
            {
                var hashBytes = pbkdf2.GetBytes(HashSize);
                return string.Format(
                    "{0}${1}${2}${3}",
                    Prefix,
                    Iterations,
                    Convert.ToBase64String(salt),
                    Convert.ToBase64String(hashBytes));
            }
        }

        public static bool Verify(string rawPassword, string storedPassword)
        {
            if (string.IsNullOrWhiteSpace(storedPassword))
            {
                return false;
            }

            if (storedPassword.StartsWith(Prefix + "$", StringComparison.Ordinal))
            {
                return VerifyPbkdf2(rawPassword, storedPassword);
            }

            var legacySha256 = LegacySha256(rawPassword);
            return string.Equals(storedPassword, rawPassword, StringComparison.Ordinal)
                || string.Equals(storedPassword, legacySha256, StringComparison.Ordinal);
        }

        private static bool VerifyPbkdf2(string rawPassword, string storedPassword)
        {
            var parts = storedPassword.Split('$');
            if (parts.Length != 4)
            {
                return false;
            }

            if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var storedHash = Convert.FromBase64String(parts[3]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(rawPassword ?? string.Empty, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var computedHash = pbkdf2.GetBytes(storedHash.Length);
                    return FixedTimeEquals(computedHash, storedHash);
                }
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string LegacySha256(string rawPassword)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(rawPassword ?? string.Empty);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}
