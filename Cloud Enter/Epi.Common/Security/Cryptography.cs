using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Configuration;
using Epi.Common.Security.Constants;

namespace Epi.Common.Security
{
    /// <summary>
    /// This class contains two static methods one to entrypt string and one to decrypt string
    /// </summary>
    public class Cryptography
    {
        public static readonly string passPhrase = SecurityAppSettings.GetStringValue(SecurityAppSettings.Key.KeyForConnectionStringPassphrase);
        public static readonly string saltValue = SecurityAppSettings.GetStringValue(SecurityAppSettings.Key.KeyForConnectionStringSalt);
        public static readonly string initVector = SecurityAppSettings.GetStringValue(SecurityAppSettings.Key.KeyForConnectionStringVector);

        /// <summary>
        /// Encryption
        /// </summary>
        /// <param name="plainText">The plaintext to encrypt</param>
        /// <returns>The ciphertext</returns>
        public static string Encrypt(string plainText)
        {
            string cipherText = null;
            try
            {
                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, "MD5", 1);
                byte[] keyBytes = password.GetBytes(16);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                cipherText = Convert.ToBase64String(cipherTextBytes);
                return cipherText;
            }
            catch (System.Exception ex)
            {
                return cipherText;
            }
        }

        /// <summary>
        /// Decryption
        /// </summary>
        /// <param name="cipherText">The ciphertext to decrypt</param>
        /// <returns>The plaintext</returns>
        public static string Decrypt(string cipherText)
        {
            string plainText = cipherText;
            try
            {
                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, "MD5", 1);
                byte[] keyBytes = password.GetBytes(16);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                return plainText;
            }
            catch (System.Exception ex)
            {
                return plainText;
            }
        }
    }
}
