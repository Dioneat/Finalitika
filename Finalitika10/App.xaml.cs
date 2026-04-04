using Finalitika10.Views; 

namespace Finalitika10
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            window.Created += (s, e) => ShowLockScreenIfNeeded();
            window.Resumed += (s, e) => ShowLockScreenIfNeeded();

            return window;
        }

        private void ShowLockScreenIfNeeded()
        {
            bool hasPin = Preferences.Default.Get("HasPinCode", false);

            if (hasPin && Application.Current?.MainPage != null)
            {
                var modalStack = Application.Current.MainPage.Navigation.ModalStack;

                if (!modalStack.Any(p => p is AppPinPage))
                {
                    Application.Current.MainPage.Navigation.PushModalAsync(new AppPinPage(), animated: false);
                }
            }
        }
    }
}