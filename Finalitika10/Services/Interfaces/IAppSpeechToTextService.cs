namespace Finalitika10.Services.Interfaces
{
    public interface IAppSpeechToTextService
    {
        Task<string?> ListenOnceAsync(string cultureName = "ru-RU", CancellationToken cancellationToken = default);
        Task StopListeningAsync(CancellationToken cancellationToken = default);
    }
}
