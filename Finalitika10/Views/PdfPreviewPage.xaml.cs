using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class PdfPreviewPage : ContentPage
{
	public PdfPreviewPage(PdfPreviewViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}