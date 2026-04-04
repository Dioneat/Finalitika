using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using Finalitika10.Views;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.AnalysisViewModels
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly ICategoryService _categoryService;
        private bool _isUpdatePending = false;

        public ObservableCollection<TransactionCategory> Categories { get; } = new();

        public CategoriesViewModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
            RequestDataUpdate();

            WeakReferenceMessenger.Default.Register<CategoryUpdatedMessage>(this, (r, m) => RequestDataUpdate());
        }

        private async void RequestDataUpdate()
        {
            if (_isUpdatePending) return;
            _isUpdatePending = true;

            await Task.Delay(50);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await LoadCategoriesAsync();
                }
                finally
                {
                    _isUpdatePending = false;
                }
            });
        }

        private async Task LoadCategoriesAsync()
        {
            Categories.Clear();

            var allCategories = await _categoryService.GetCategoriesAsync();
            var sortedCategories = allCategories.OrderByDescending(c => c.Type).ToList();

            foreach (var cat in sortedCategories)
            {
                Categories.Add(cat);
            }
        }

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            var vm = new AddEditCategoryViewModel(_categoryService, null);
            await Shell.Current.Navigation.PushModalAsync(new AddEditCategoryPage(vm));
        }

        [RelayCommand]
        private async Task EditCategoryAsync(TransactionCategory category)
        {
            if (category == null) return;
            var vm = new AddEditCategoryViewModel(_categoryService, category);
            await Shell.Current.Navigation.PushModalAsync(new AddEditCategoryPage(vm));
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync(TransactionCategory category)
        {
            if (category == null) return;

            if (category.IsDefault)
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Базовые категории нельзя удалить.", "ОК");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlertAsync("Удаление", $"Удалить категорию '{category.Name}'?", "Да", "Отмена");
            if (confirm)
            {
                await _categoryService.DeleteCategoryAsync(category);
                WeakReferenceMessenger.Default.Send(new CategoryUpdatedMessage());
            }
        }
    }
}