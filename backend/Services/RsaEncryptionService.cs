using System.Security.Cryptography;
using System.Text;

namespace backend.Services
{
    public class RsaEncryptionService
    {
        private readonly RSA _rsa;

        // 🔥 STATIC KEY (Generated once)
        private const string PrivateKeyBase64 = "MIIEogIBAAKCAQEAsUKxHFZV8VlRJWHxuoBjgRsxLFCPLGprNEvrDDPIR9bVFvLKiZZns8UHI4WUBftFizVbbt/Xumd2eORzdn/PEnlgtqBliNWALbQRzyiNP6/2+KcL1dbGvG/Gq5dLEXvlyconAgRrq+v8VgFw1gbj/K0AbyjEAVW3ttr7g/594KH35KMRzFkRwfIuFi/3VQIhxzCWGwwMll+CEWS43fxJwxHS2krGm1djp+QfbmCAWE+0Bh3i0VpkkHYxkenaa1oadfhJbxDny6WlIHNFi2y3OykFA07/OzB7SqCCBJLEZPj+TLj+Y5A4yJJkz217iWo148l/jAHNXQx4eHwlZMD4vQIDAQABAoIBAAD4k43LL0dWLOqegbt6zpC7fGZ4voAIyTONIul7MyKpm5s3k0Jzr3e9oY9hQPFTcgsULL8LvNvJi02YOsWwZ4r7XTtC36uDN+OIM/zFhRiPNzQffiIavhX0Kstv1bpvyk9zqwkIyyoy7bvu4BZcDU2qCPGS8JftR0daJbyQXVIorqQChW5wPLUUxRNhZ4BEO+S6kCO5znxj86WEH3y/2H1mglSXb3tnmqQqkcmcH7HlGr0ePpGhA45++8lA3JUXjTpgx0sRJ4k6rNlYVK26hl/VITPveNrvsa62pJ/kS3aLRexHOC83KTi5Eps2ixX4LTwAq5CestX2IYtvlHUXY2UCgYEA3bUeMa+74TtF+1W1ap9b9hqiEvlbM8+tb+N4ceSko5oKR361FCPT2KfMWj5NzLNE6aW3JIDx+7OnmcY/NJ6Ucvl0kWnSdhrAUxcPySWhMeaRu2FzpneXXT92FZTceILHX+q/nk2FeRskEL+A9XVesCTHH0NfUXlk/StIRzlexiMCgYEAzK2e8ffFBMztsNyo/C/G29WXKcoE1zWgN6O/WZr6EDxIoK3B2zogY/GuQ/2BugyVZD7l5Z/bzFTvMaeM0J5TVNFeKMyXJorzhsqCIcroryXhbIb9uXQNn4kmnfiu6ZeB3XRptsXt6dUnf1tnrO7G65ck6bQfqcUD0puNyyaAg58CgYBKV+mAD8WYiR+2X88tqbJUnCms4yDKBuHJmyVQS7NJiOXZg4uE7V6kT+AegrXj5lk9X+xBuVtrY5rX3bpkEqkExNFp0WpZYC8NrbYFZTDgYsiLZOo67+85vYYDRp+HaZMhfr4yfxw6t/coBo9fv7O4rRx4R0c3MtXCbzkVaQ92sQKBgFGnPg6n2sh0fKqfDSc2ZC3VQu3f8xhsA1PPFopHwlTxx8OaLgkoYj2c7/mdy8AOPxbz5ME7ifM1OwtjHVEoqHPg5qB+JhqxOj/e0BVtHnCpOwoRvGzcuIaZr3c7+4Oi9qMDSx0oTyaG66/qJi0eQOtBq+8z9aHDwCen+OQ/outfAoGAfMn7rjDEJdLZmWF2pGPKK8Uy0nL/yLvzM0Y8feQXHhviFs2UdMGD/Siyj1tBRDRZItj4wsn3qeaFLptfiOoGXS7JX92Gxj3M0tq+7v+4tvQ4bWZNguBXvb3jvJHiCquDfK48pUqp2Uj32vsxtHbP/Dzv2M7RYhCRgDPWE2H5rFM=";

        public RsaEncryptionService()
        {
            _rsa = RSA.Create();
            _rsa.ImportRSAPrivateKey(
                Convert.FromBase64String(PrivateKeyBase64),
                out _
            );
        }

        public string Encrypt(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = _rsa.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string encryptedText)
        {
            var bytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = _rsa.Decrypt(bytes, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public string GenerateHash(string plainText)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
