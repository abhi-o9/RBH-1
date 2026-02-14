using System.Security.Cryptography;
using System.Text;

namespace backend.Services
{
    public class RsaEncryptionService
    {
        private readonly RSA _rsa;

        public RsaEncryptionService()
        {
            _rsa = RSA.Create(2048);
        }

        // Encrypt text using RSA
        public string Encrypt(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = _rsa.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encryptedBytes);
        }

        // Decrypt text using RSA
        public string Decrypt(string encryptedText)
        {
            var bytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = _rsa.Decrypt(bytes, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        // Generate SHA256 hash
        public string GenerateHash(string plainText)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
