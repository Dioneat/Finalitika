using Finalitika10.ViewModels.AnalysisViewModels;

namespace Finalitika10.Views;

public partial class CategoriesPage : ContentPage
{
	public CategoriesPage(CategoriesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}