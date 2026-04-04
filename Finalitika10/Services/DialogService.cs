namespace Finalitika10.Services.AppServices
{
    public sealed class DialogService : IDialogService
    {
        private static Page CurrentPage =>
            Shell.Current?.CurrentPage
            ?? Application.Current?.Windows.FirstOrDefault()?.Page
            ?? throw new InvalidOperationException("Активная страница не найдена.");

        public Task AlertAsync(string title, string message, string cancel = "ОК") =>
            CurrentPage.DisplayAlertAsync(title, message, cancel);

        public Task<string?> PromptAsync(
            string title,
            string message,
            string accept = "ОК",
            string cancel = "Отмена",
            string? placeholder = null,
            int maxLength = -1,
            Keyboard? keyboard = null,
            string initialValue = "") =>
            CurrentPage.DisplayPromptAsync(
                title,
                message,
                accept,
                cancel,
                placeholder,
                maxLength,
                keyboard ?? Keyboard.Default,
                initialValue);
    }
}