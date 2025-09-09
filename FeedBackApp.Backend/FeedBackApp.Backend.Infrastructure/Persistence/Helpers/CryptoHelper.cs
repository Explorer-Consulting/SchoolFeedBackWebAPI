
using System.Security.Cryptography;
using System.Text;

namespace FeedBackApp.Backend.Infrastructure.Persistence.Helpers
{
    // helper class for encrypt/decrypt plaintext (deterministic)
    public static class CryptoHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("ENCRYPTION_KEY") ?? throw new InvalidOperationException("ENCRYPTION_KEY not set"));
        private static readonly CipherMode Mode = CipherMode.ECB;
        private static readonly PaddingMode Padding = PaddingMode.PKCS7;

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.Mode = Mode;
            aes.Padding = Padding;

            using var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.Mode = Mode;
            aes.Padding = Padding;

            using var decryptor = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(cipherText);
            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }

    }
}
