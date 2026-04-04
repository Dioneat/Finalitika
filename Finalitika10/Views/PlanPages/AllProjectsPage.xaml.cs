using Finalitika10.ViewModels.PlanViewModels;

namespace Finalitika10.Views.PlanPages;

public partial class AllProjectsPage : ContentPage
{
	public AllProjectsPage(AllProjectsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}