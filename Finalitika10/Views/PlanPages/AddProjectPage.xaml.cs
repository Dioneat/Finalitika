using Finalitika10.ViewModels.PlanViewModels;

namespace Finalitika10.Views.PlanPages;

public partial class AddProjectPage : ContentPage
{
	public AddProjectPage(AddProjectViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}