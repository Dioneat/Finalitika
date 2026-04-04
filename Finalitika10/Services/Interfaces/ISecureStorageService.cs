namespace Finalitika10.Services.AppServices
{
    public interface ISecureStorageService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value);
        bool Remove(string key);
    }
}