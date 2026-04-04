using Finalitika10.ViewModels.AnalysisViewModels;

namespace Finalitika10.Views;

public partial class AddTransferPage : ContentPage
{
	public AddTransferPage(AddTransferViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}