using Finalitika10.ViewModels.AnalysisViewModels;

namespace Finalitika10.Views;

public partial class AddEditCategoryPage : ContentPage
{
	public AddEditCategoryPage(AddEditCategoryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}