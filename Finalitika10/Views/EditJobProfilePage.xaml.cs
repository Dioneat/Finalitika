using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class EditJobProfilePage : ContentPage
{
	public EditJobProfilePage(EditJobProfileViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}