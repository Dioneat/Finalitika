using Finalitika10.ViewModels.PlanViewModels;

namespace Finalitika10.Views.PlanPages;

public partial class AddPaymentPage : ContentPage
{
	public AddPaymentPage(AddPaymentViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}