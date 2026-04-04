using Finalitika10.ViewModels.AnalysisViewModels;

namespace Finalitika10.Views;

public partial class AddEditAccountPage : ContentPage
{
	public AddEditAccountPage(AddEditAccountViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}