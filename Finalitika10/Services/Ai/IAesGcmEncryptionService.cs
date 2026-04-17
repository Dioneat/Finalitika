using System;
using System.Collections.Generic;
using System.Text;

namespace Finalitika10.Services.Ai
{
    public interface IAesGcmEncryptionService
    {
        Task<EncryptedData> EncryptAsync(string plainText, CancellationToken cancellationToken = default);
        Task<string> DecryptAsync(EncryptedData encryptedData, CancellationToken cancellationToken = default);
    }

    public sealed class EncryptedData
    {
        public string CipherTextBase64 { get; init; } = string.Empty;
        public string NonceBase64 { get; init; } = string.Empty;
        public string TagBase64 { get; init; } = string.Empty;
    }
}
