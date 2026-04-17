using Finalitika10.Services.AppServices;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Finalitika10.Services.Ai
{
    public sealed class AesGcmEncryptionService : IAesGcmEncryptionService
    {
        private const string EncryptionKeyStorageKey = "ai_chat_history_aes_key";
        private const int KeySizeBytes = 32;   // 256-bit
        private const int NonceSizeBytes = 12; // recommended for GCM
        private const int TagSizeBytes = 16;   // 128-bit tag

        private readonly ISecureStorageService _secureStorageService;

        public AesGcmEncryptionService(ISecureStorageService secureStorageService)
        {
            _secureStorageService = secureStorageService;
        }

        public async Task<EncryptedData> EncryptAsync(string plainText, CancellationToken cancellationToken = default)
        {
            if (plainText is null)
                throw new ArgumentNullException(nameof(plainText));

            byte[] key = await GetOrCreateKeyAsync();
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSizeBytes];

            using var aes = new AesGcm(key);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            return new EncryptedData
            {
                CipherTextBase64 = Convert.ToBase64String(ciphertext),
                NonceBase64 = Convert.ToBase64String(nonce),
                TagBase64 = Convert.ToBase64String(tag)
            };
        }

        public async Task<string> DecryptAsync(EncryptedData encryptedData, CancellationToken cancellationToken = default)
        {
            if (encryptedData is null)
                throw new ArgumentNullException(nameof(encryptedData));

            byte[] key = await GetOrCreateKeyAsync();
            byte[] nonce = Convert.FromBase64String(encryptedData.NonceBase64);
            byte[] ciphertext = Convert.FromBase64String(encryptedData.CipherTextBase64);
            byte[] tag = Convert.FromBase64String(encryptedData.TagBase64);
            byte[] plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(key);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }

        private async Task<byte[]> GetOrCreateKeyAsync()
        {
            string? savedKey = await _secureStorageService.GetAsync(EncryptionKeyStorageKey);

            if (!string.IsNullOrWhiteSpace(savedKey))
            {
                return Convert.FromBase64String(savedKey);
            }

            byte[] key = RandomNumberGenerator.GetBytes(KeySizeBytes);
            await _secureStorageService.SetAsync(EncryptionKeyStorageKey, Convert.ToBase64String(key));

            return key;
        }
    }
}
