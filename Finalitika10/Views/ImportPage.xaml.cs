using Finalitika10.ViewModels.AnalysisViewModels;

namespace Finalitika10.Views;

public partial class ImportPage : ContentPage
{
	public ImportPage(ImportViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}