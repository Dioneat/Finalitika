using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class AddCurrencyPage : ContentPage
{
	public AddCurrencyPage(AddCurrencyViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}