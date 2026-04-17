namespace Finalitika10.Views
{
    public partial class TypingIndicator : ContentView
    {
        public static readonly BindableProperty IsAnimatingProperty =
            BindableProperty.Create(
                nameof(IsAnimating),
                typeof(bool),
                typeof(TypingIndicator),
                false,
                propertyChanged: OnIsAnimatingChanged);

        private CancellationTokenSource? _animationCts;

        public bool IsAnimating
        {
            get => (bool)GetValue(IsAnimatingProperty);
            set => SetValue(IsAnimatingProperty, value);
        }

        public TypingIndicator()
        {
            InitializeComponent();
        }

        private static void OnIsAnimatingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not TypingIndicator indicator || newValue is not bool isAnimating)
                return;

            if (isAnimating)
            {
                indicator.StartAnimation();
            }
            else
            {
                indicator.StopAnimation();
            }
        }

        private void StartAnimation()
        {
            StopAnimation();

            _animationCts = new CancellationTokenSource();
            _ = AnimateAsync(_animationCts.Token);
        }

        private void StopAnimation()
        {
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;

            ResetDots();
        }

        private async Task AnimateAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await JumpDotAsync(Dot1, cancellationToken);
                    await Task.Delay(120, cancellationToken);

                    await JumpDotAsync(Dot2, cancellationToken);
                    await Task.Delay(120, cancellationToken);

                    await JumpDotAsync(Dot3, cancellationToken);
                    await Task.Delay(300, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                ResetDots();
            }
        }

        private static async Task JumpDotAsync(VisualElement dot, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await dot.TranslateToAsync(0, -8, 160, Easing.CubicOut);
            cancellationToken.ThrowIfCancellationRequested();
            await dot.TranslateToAsync(0, 0, 160, Easing.CubicIn);
        }

        private void ResetDots()
        {
            Dot1.TranslationY = 0;
            Dot2.TranslationY = 0;
            Dot3.TranslationY = 0;
        }
    }
}