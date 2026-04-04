using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class AllOperationsPage : ContentPage
{
	public AllOperationsPage(AllOperationsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}