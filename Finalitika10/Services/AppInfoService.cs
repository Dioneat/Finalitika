namespace Finalitika10.Services
{
    public interface IAppInfoService
    {
        string Version { get; }
    }
    public sealed class AppInfoService : IAppInfoService
    {
        public string Version =>
            $"Версия {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";
    }
}
