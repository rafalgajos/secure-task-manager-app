using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace secure_task_manager_app.Services
{
    public static class SecureTokenStorage
    {
        // Path to the file where the encrypted token will be stored
        private static readonly string FilePath = Path.Combine(Path.GetTempPath(), "token.dat");

        // Key and IV values - in production should be generated securely and stored in secure storage.
        private static readonly byte[] Key = GenerateKey(32); // AES-256 requires a key length of 32 bytes.
        private static readonly byte[] IV = GenerateIV(16);   // AES requires an IV of 16 bytes in length.

        /// <summary>
        /// Saves the encrypted token in a file.
        /// </summary>
        public static async Task SaveTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        await sw.WriteAsync(token);
                    }

                    var encryptedToken = ms.ToArray();
                    await File.WriteAllBytesAsync(FilePath, encryptedToken);
                }
            }
        }

        /// <summary>
        /// Reads and decrypts the token from the file.
        /// </summary>
        public static async Task<string> GetTokenAsync()
        {
            if (!File.Exists(FilePath))
                return null;

            var encryptedToken = await File.ReadAllBytesAsync(FilePath);

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = IV;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(encryptedToken))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }
            }
            catch (CryptographicException)
            {
                // Return null if token is corrupted or incorrectly decrypted
                Console.WriteLine("Error: Invalid padding or corrupted token. Unable to retrieve token.");
                return null;
            }
        }

        /// <summary>
        /// Removes the encrypted token from the file.
        /// </summary>
        public static void ClearToken()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        /// <summary>
        /// Generates a key of the specified length in bytes.
        /// Use secure key storage in production.
        /// </summary>
        private static byte[] GenerateKey(int size)
        {
            var key = new byte[size];
            RandomNumberGenerator.Fill(key); // Generates a random key
            return key;
        }

        /// <summary>
        /// Generates an IV of the specified length in bytes.
        /// </summary>
        private static byte[] GenerateIV(int size)
        {
            var iv = new byte[size];
            RandomNumberGenerator.Fill(iv); // Generates a random IV
            return iv;
        }
    }
}
