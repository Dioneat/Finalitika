using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class AIChatPage : ContentPage
{
    private readonly AIChatViewModel _viewModel;

    public AIChatPage(AIChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}