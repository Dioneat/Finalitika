using Finalitika10.ViewModels.AnalysisViewModels;

namespace Finalitika10.Views;

public partial class AccountsPage : ContentPage
{
	public AccountsPage(AccountsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}