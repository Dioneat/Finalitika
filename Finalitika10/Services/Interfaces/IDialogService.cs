namespace Finalitika10.Services.AppServices
{
    public interface IDialogService
    {
        Task AlertAsync(string title, string message, string cancel = "ОК");
        Task<bool> ConfirmAsync(
            string title,
            string message,
            string accept = "ОК",
            string cancel = "Отмена");
        Task<string?> PromptAsync(
            string title,
            string message,
            string accept = "ОК",
            string cancel = "Отмена",
            string? placeholder = null,
            int maxLength = -1,
            Keyboard? keyboard = null,
            string initialValue = "");
    }
}