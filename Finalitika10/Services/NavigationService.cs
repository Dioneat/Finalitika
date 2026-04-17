namespace Finalitika10.Services
{
    public interface INavigationService
    {
        Task GoToAsync(string route);
    }

    public sealed class NavigationService : INavigationService
    {
        public Task GoToAsync(string route) =>
            Shell.Current?.GoToAsync(route) ?? Task.CompletedTask;
    }
}
