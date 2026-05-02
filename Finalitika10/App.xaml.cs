using Finalitika10.Views;

namespace Finalitika10
{
    public partial class App : Application
    {
        private readonly OnboardingPage _onboardingPage;

        public App(OnboardingPage onboardingPage)
        {
            InitializeComponent();
            _onboardingPage = onboardingPage;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // 1. Проверяем, прошел ли пользователь первоначальную настройку
            bool isOnboardingComplete = Preferences.Default.Get("IsOnboardingComplete", false);

            // 2. В зависимости от статуса выбираем стартовую страницу
            Page rootPage = isOnboardingComplete ? new AppShell() : _onboardingPage;

            var window = new Window(rootPage);

            // 3. Подписываемся на события жизненного цикла
            window.Created += (s, e) => ShowLockScreenIfNeeded();
            window.Resumed += (s, e) => ShowLockScreenIfNeeded();

            return window;
        }

        private void ShowLockScreenIfNeeded()
        {
            bool isOnboardingComplete = Preferences.Default.Get("IsOnboardingComplete", false);
            if (!isOnboardingComplete)
                return;

            // Проверяем наличие ПИН-кода
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