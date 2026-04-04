using Finalitika10.ViewModels.PlanViewModels;

namespace Finalitika10.Views;

public partial class AllNotesPage : ContentPage
{
	public AllNotesPage(AllNotesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}