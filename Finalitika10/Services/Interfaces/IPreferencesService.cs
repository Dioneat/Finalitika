namespace Finalitika10.Services.AppServices
{
    public interface IPreferencesService
    {
        bool GetBool(string key, bool defaultValue = false);
        void SetBool(string key, bool value);

        string GetString(string key, string defaultValue = "");
        void SetString(string key, string value);
    }
}