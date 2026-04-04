using Finalitika10.ViewModels.PlanViewModels;

namespace Finalitika10.Views.PlanPages;

public partial class ProjectDetailPage : ContentPage
{
	public ProjectDetailPage(ProjectDetailViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}