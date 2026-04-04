using Finalitika10.ViewModels.PlanViewModels;

namespace Finalitika10.Views.PlanPages;

public partial class AllPaymentsPage : ContentPage
{
	public AllPaymentsPage(AllPaymentsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}