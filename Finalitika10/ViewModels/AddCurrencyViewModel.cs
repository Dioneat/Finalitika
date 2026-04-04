using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Services.Investments;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels
{
    public partial class AddCurrencyViewModel : ObservableObject
    {
        private readonly ICurrencyService _currencyService;
        private List<CurrencyItem> _allCurrencies = new();

        public ObservableCollection<CurrencyItem> DisplayedCurrencies { get; } = new();

        [ObservableProperty]
        private bool isLoading;

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    SearchCurrencies();
                }
            }
        }

        public AddCurrencyViewModel(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
            LoadDataAsync();
        }

        [RelayCommand]
        private async Task AddCurrencyAsync(CurrencyItem item)
        {
            if (item == null) return;

            _currencyService.AddTrackedCurrency(item.Id);

            await Shell.Current.DisplayAlert("Успех", $"{item.Name} добавлена на главную страницу!", "ОК");

            await Shell.Current.GoToAsync("..");
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                _allCurrencies = await _currencyService.GetAvailableCurrenciesAsync();
                SearchCurrencies();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void SearchCurrencies()
        {
            var query = SearchQuery ?? string.Empty;

            DisplayedCurrencies.Clear();

            var filtered = string.IsNullOrWhiteSpace(query)
                ? _allCurrencies
                : _allCurrencies.Where(c =>
                    c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    c.EngName.Contains(query, StringComparison.OrdinalIgnoreCase));

            foreach (var item in filtered)
            {
                DisplayedCurrencies.Add(item);
            }
        }
    }
}