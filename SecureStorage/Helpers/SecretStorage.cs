
using System.Security.Cryptography;

namespace SecureStorage.Helpers
{
    public static class SecretStorage
    {
        public static string EncryptData(string data, byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(data);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string DecryptData(string encryptedData, byte[] key)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                byte[] iv = new byte[16];
                if (encryptedBytes.Length < iv.Length)
                    throw new ArgumentException("Encrypted data is too short to contain IV.");

                Buffer.BlockCopy(encryptedBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}