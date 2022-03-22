using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MessengerAPI.Services
{
    public class CryptographyService
    {
        public string EncryptString(string toEncrypt)
        {
            if (toEncrypt == null)
                throw new ArgumentNullException("toEncrypt");

            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("iKg$Leez+3OaW-V7J8FpI/zKX9tK_aXl");
                aes.IV = Encoding.UTF8.GetBytes("/qS?i7;E+lObld+m");
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream msEncrypt = new MemoryStream();
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(toEncrypt);
                }
                encrypted = msEncrypt.ToArray();
            }

            return Convert.ToBase64String(encrypted);
        }

        public string DecryptString(string value)
        {
            byte[] toDecrypt = Convert.FromBase64String(value);
            if (toDecrypt == null)
                throw new ArgumentNullException("toDecrypt");

            string decrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("iKg$Leez+3OaW-V7J8FpI/zKX9tK_aXl");
                aes.IV = Encoding.UTF8.GetBytes("/qS?i7;E+lObld+m");
                ICryptoTransform encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream msDecrypt = new MemoryStream(toDecrypt);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, encryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);
                decrypted = srDecrypt.ReadToEnd();
            }
            return decrypted;
        }

        public string EncryptMessage(string toEncrypt)
        {
            if (toEncrypt == null)
                throw new ArgumentNullException("toEncrypt");

            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("QPjknmdyT14hj!a@");
                aes.IV = Encoding.UTF8.GetBytes("123qSd74hgTysdtv");
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream msEncrypt = new MemoryStream();
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(toEncrypt);
                }
                encrypted = msEncrypt.ToArray();
            }

            return Convert.ToBase64String(encrypted);
        }

        public string DecryptMessage(string value)
        {
            byte[] toDecrypt = Convert.FromBase64String(value);
            if (toDecrypt == null)
                throw new ArgumentNullException("toDecrypt");

            string decrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("QPjknmdyT14hj!a@");
                aes.IV = Encoding.UTF8.GetBytes("123qSd74hgTysdtv");
                ICryptoTransform encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream msDecrypt = new MemoryStream(toDecrypt);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, encryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);
                decrypted = srDecrypt.ReadToEnd();
            }

            return decrypted;
        }

        public string EncryptForgot(string value)
        {
            if (value == null)
                throw new ArgumentNullException("toEncrypt");

            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("fc5'Ps]jrQ6f&alx");
                aes.IV = Encoding.UTF8.GetBytes("dur4DY{kja9tmz!1");
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream msEncrypt = new MemoryStream();
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(value);
                }
                encrypted = msEncrypt.ToArray();
            }
            return Convert.ToBase64String(encrypted);
        }

        public string DecryptForgot(string value)
        {
            byte[] toDecrypt = Convert.FromBase64String(value);
            if (toDecrypt == null)
                throw new ArgumentNullException("toDecrypt");

            string decrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes("fc5'Ps]jrQ6f&alx");
                aes.IV = Encoding.UTF8.GetBytes("dur4DY{kja9tmz!1");
                ICryptoTransform encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream msDecrypt = new MemoryStream(toDecrypt);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, encryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);
                decrypted = srDecrypt.ReadToEnd();
            }

            return decrypted;
        }

        public string CreateString(int bytesNumber)
        {
            var rand = new Random();
            var bytes = new byte[bytesNumber];
            var bytes2 = new byte[bytesNumber/2];
            rand.NextBytes(bytes);
            rand.NextBytes(bytes2);
            string v1 = Convert.ToBase64String(bytes);
            string v2 = v1.Remove(8);
            string v3 = Convert.ToBase64String(bytes2);
            string v4 = v3.Remove(4);
            string v5 = v2 + v4;
            return v5;
        }

        public int CreateNumber(int min, int max) => RandomNumberGenerator.GetInt32(min, max);

    }
}
