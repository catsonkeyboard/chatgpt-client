using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatWpfUI.Common
{
    public class AesCrypto
    {
        private static byte[] IV = new byte[] { /* 初始化向量 */ };

        public static string Encrypt(string plaintext, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 128; // 设置密钥长度
                aes.Key = keyBytes;
                aes.IV = IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                    }

                    byte[] ciphertextBytes = ms.ToArray();
                    return Convert.ToBase64String(ciphertextBytes);
                }
            }
        }

        public static string Decrypt(string ciphertext, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ciphertextBytes = Convert.FromBase64String(ciphertext);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 128; // 设置密钥长度
                aes.Key = keyBytes;
                aes.IV = IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(ciphertextBytes, 0, ciphertextBytes.Length);
                    }

                    byte[] plaintextBytes = ms.ToArray();
                    return Encoding.UTF8.GetString(plaintextBytes);
                }
            }
        }
    }
}
