using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Finalitika10.Services
{
    public interface IToastService
    {
        Task ShowAsync(string message);
    }

    public sealed class ToastService : IToastService
    {
        public async Task ShowAsync(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Short, 14);
            await toast.Show();
        }
    }
}
