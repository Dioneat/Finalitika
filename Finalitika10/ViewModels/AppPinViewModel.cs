using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace Finalitika10.ViewModels
{
    public partial class AppPinViewModel : ObservableObject
    {
        [ObservableProperty] private string enteredPin = "";
        [ObservableProperty] private string pinDots = "○ ○ ○ ○"; 
        [ObservableProperty] private string statusMessage = "Введите ПИН-код";
        [ObservableProperty] private string statusColor = "#2C3E50"; 
        [ObservableProperty] private bool isBiometricVisible;

        public AppPinViewModel()
        {
            IsBiometricVisible = Preferences.Default.Get("UseBiometrics", false);

            if (IsBiometricVisible)
            {
                TriggerBiometricAsync();
            }
        }

        [RelayCommand]
        private async Task NumberPressAsync(string number)
        {
            if (EnteredPin.Length < 4)
            {
                EnteredPin += number;
                UpdateDots();

                if (EnteredPin.Length == 4)
                {
                    await ValidatePinAsync();
                }
            }
        }

        [RelayCommand]
        private void DeletePress()
        {
            if (EnteredPin.Length > 0)
            {
                EnteredPin = EnteredPin.Substring(0, EnteredPin.Length - 1);
                UpdateDots();
                StatusMessage = "Введите ПИН-код";
                StatusColor = "#2C3E50";
            }
        }

        [RelayCommand]
        public async Task TriggerBiometricAsync()
        {
            var isAvailable = await CrossFingerprint.Current.IsAvailableAsync();
            if (!isAvailable) return;

            var request = new AuthenticationRequestConfiguration("Вход в Finalitika", "Подтвердите личность для входа");
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);

            if (result.Authenticated)
            {
                UnlockApp();
            }
        }

        private async Task ValidatePinAsync()
        {
            string savedPin = await SecureStorage.Default.GetAsync("app_pin_code");

            if (EnteredPin == savedPin)
            {
                StatusMessage = "Успешно!";
                StatusColor = "#27AE60";
                await Task.Delay(200); 
                UnlockApp();
            }
            else
            {
                StatusMessage = "Неверный ПИН-код";
                StatusColor = "#E74C3C";
                EnteredPin = "";
                UpdateDots();
                HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); 
            }
        }

        private void UpdateDots()
        {
            string dots = "";
            for (int i = 0; i < 4; i++)
            {
                dots += i < EnteredPin.Length ? "● " : "○ ";
            }
            PinDots = dots.TrimEnd();
        }

        private void UnlockApp()
        {
            Application.Current.MainPage.Navigation.PopModalAsync(animated: false);
        }
    }
}