using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class DocumentsPage : ContentPage
{
	public DocumentsPage(DocumentsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}