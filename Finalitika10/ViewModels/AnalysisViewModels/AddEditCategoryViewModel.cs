using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;

namespace Finalitika10.ViewModels.AnalysisViewModels
{
    public partial class AddEditCategoryViewModel : ObservableObject
    {
        private readonly ICategoryService _categoryService;
        private readonly TransactionCategory _editingCategory;

        public List<string> CategoryTypes { get; } = new() { "Расход", "Доход" };

        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string name;
        [ObservableProperty] private string icon = "🏷️";
        [ObservableProperty] private string selectedType = "Расход";

        public AddEditCategoryViewModel(ICategoryService categoryService, TransactionCategory existingCategory)
        {
            _categoryService = categoryService;
            _editingCategory = existingCategory;

            if (_editingCategory != null)
            {
                PageTitle = "Редактирование категории";
                Name = _editingCategory.Name;
                Icon = _editingCategory.Icon;
                SelectedType = _editingCategory.Type;
            }
            else
            {
                PageTitle = "Новая категория";
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Введите название категории", "ОК");
                return;
            }

            var category = _editingCategory ?? new TransactionCategory { ColorHex = GetRandomColor(), IsDefault = false };

            category.Name = Name;
            category.Icon = string.IsNullOrWhiteSpace(Icon) ? "🏷️" : Icon;
            category.Type = SelectedType;

            await _categoryService.SaveCategoryAsync(category);
            WeakReferenceMessenger.Default.Send(new CategoryUpdatedMessage());

            await Shell.Current.Navigation.PopModalAsync();
        }

        [RelayCommand]
        private async Task CloseAsync()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private string GetRandomColor()
        {
            var colors = new[] { "#34495E", "#27AE60", "#2980B9", "#8E44AD", "#E67E22", "#C0392B", "#16A085", "#D35400" };
            return colors[new Random().Next(colors.Length)];
        }
    }
}
