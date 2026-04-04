using Finalitika10.ViewModels.AnalysisViewModels;

namespace Finalitika10.Views;

public partial class AddTransactionPage : ContentPage
{
	public AddTransactionPage(AddTransactionViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;	
	}
}