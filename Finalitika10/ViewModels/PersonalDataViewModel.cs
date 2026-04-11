using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Finalitika10.ViewModels
{
    public partial class PersonalDataViewModel : ObservableObject
    {
        [ObservableProperty] private string lastName = "";
        [ObservableProperty] private string firstName = "";
        [ObservableProperty] private string patronymic = "";

        [ObservableProperty] private string phone = "";
        [ObservableProperty] private string birthDate = "";
        [ObservableProperty] private string address = "";

        [ObservableProperty] private string passportData = "";
        [ObservableProperty] private string inn = "";

        public PersonalDataViewModel()
        {
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            // Нечувствительные данные берем из Preferences
            LastName = Preferences.Default.Get("User_LastName", "");
            FirstName = Preferences.Default.Get("User_FirstName", "");
            Patronymic = Preferences.Default.Get("User_Patronymic", "");
            Phone = Preferences.Default.Get("User_Phone", "");
            BirthDate = Preferences.Default.Get("User_BirthDate", "");
            Address = Preferences.Default.Get("User_Address", "");

            PassportData = await SecureStorage.Default.GetAsync("User_Passport") ?? "";
            Inn = await SecureStorage.Default.GetAsync("User_INN") ?? "";
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            Preferences.Default.Set("User_LastName", LastName);
            Preferences.Default.Set("User_FirstName", FirstName);
            Preferences.Default.Set("User_Patronymic", Patronymic);
            Preferences.Default.Set("User_Phone", Phone);
            Preferences.Default.Set("User_BirthDate", BirthDate);
            Preferences.Default.Set("User_Address", Address);

            await SecureStorage.Default.SetAsync("User_Passport", PassportData ?? "");
            await SecureStorage.Default.SetAsync("User_INN", Inn ?? "");

            await Shell.Current.DisplayAlertAsync("Сохранено", "Ваши данные надежно сохранены. Теперь они будут использоваться для автозаполнения документов.", "ОК");
            await Shell.Current.Navigation.PopAsync();
        }
    }
}