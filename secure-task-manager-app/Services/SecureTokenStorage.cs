using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace secure_task_manager_app.Services
{
    public static class SecureTokenStorage
    {
        // Ścieżka do pliku, w którym będzie przechowywany zaszyfrowany token
        private static readonly string FilePath = Path.Combine(Path.GetTempPath(), "token.dat");

        // Wartości klucza i IV - w produkcji powinny być generowane bezpiecznie i przechowywane w bezpiecznym magazynie.
        private static readonly byte[] Key = GenerateKey(32); // AES-256 wymaga klucza o długości 32 bajtów.
        private static readonly byte[] IV = GenerateIV(16);   // AES wymaga IV o długości 16 bajtów.

        /// <summary>
        /// Zapisuje zaszyfrowany token w pliku.
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
        /// Odczytuje i odszyfrowuje token z pliku.
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
                // Zwróć null, jeśli token jest uszkodzony lub niepoprawnie odszyfrowany
                Console.WriteLine("Error: Invalid padding or corrupted token. Unable to retrieve token.");
                return null;
            }
        }

        /// <summary>
        /// Usuwa zaszyfrowany token z pliku.
        /// </summary>
        public static void ClearToken()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        /// <summary>
        /// Generuje klucz o podanej długości w bajtach.
        /// W produkcji użyj bezpiecznego przechowywania kluczy.
        /// </summary>
        private static byte[] GenerateKey(int size)
        {
            var key = new byte[size];
            RandomNumberGenerator.Fill(key); // Generuje losowy klucz
            return key;
        }

        /// <summary>
        /// Generuje IV o podanej długości w bajtach.
        /// </summary>
        private static byte[] GenerateIV(int size)
        {
            var iv = new byte[size];
            RandomNumberGenerator.Fill(iv); // Generuje losowy IV
            return iv;
        }
    }
}
