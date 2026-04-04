namespace Finalitika10.Services.AppServices
{
    public sealed class PreferencesService : IPreferencesService
    {
        public bool GetBool(string key, bool defaultValue = false) =>
            Preferences.Default.Get(key, defaultValue);

        public void SetBool(string key, bool value) =>
            Preferences.Default.Set(key, value);

        public string GetString(string key, string defaultValue = "") =>
            Preferences.Default.Get(key, defaultValue);

        public void SetString(string key, string value) =>
            Preferences.Default.Set(key, value);
    }
}