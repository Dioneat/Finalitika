using Microsoft.Maui.Storage;

namespace Finalitika10.Services.AppServices
{
    public sealed class SecureStorageService : ISecureStorageService
    {
        public Task<string?> GetAsync(string key) =>
            SecureStorage.Default.GetAsync(key);

        public Task SetAsync(string key, string value) =>
            SecureStorage.Default.SetAsync(key, value);

        public bool Remove(string key) =>
            SecureStorage.Default.Remove(key);
    }
}