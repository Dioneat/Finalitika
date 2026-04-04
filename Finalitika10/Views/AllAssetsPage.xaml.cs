using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class AllAssetsPage : ContentPage
{
	public AllAssetsPage(AllAssetsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}