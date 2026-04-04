using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Services.Investments;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels
{
    public partial class AllAssetsViewModel : ObservableObject
    {
        private readonly IInvestmentService _investmentService;
        private List<PositionData> _allPositions = new(); 

        [ObservableProperty] private bool isLoading;

        public ObservableCollection<PositionData> DisplayedAssets { get; } = new();

        public AllAssetsViewModel(IInvestmentService investmentService)
        {
            _investmentService = investmentService;
            LoadAssetsAsync();
        }

        private async Task LoadAssetsAsync()
        {
            IsLoading = true;
            try
            {
                var data = await _investmentService.GetPortfolioAsync();
                _allPositions = data.Positions;

                FilterAssets("Все"); 
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", ex.Message, "ОК");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void FilterAssets(string category)
        {
            DisplayedAssets.Clear();

            var filtered = category switch
            {
                "Акции" => _allPositions.Where(p => p.InstrumentType is "share" or "shares"),
                "Облигации" => _allPositions.Where(p => p.InstrumentType is "bond" or "bonds"),
                "Фонды" => _allPositions.Where(p => p.InstrumentType is "etf" or "etfs"),
                "Валюта" => _allPositions.Where(p => p.InstrumentType is "currency" or "currencies"),
                _ => _allPositions 
            };

            foreach (var item in filtered)
            {
                DisplayedAssets.Add(item);
            }
        }
    }
}
