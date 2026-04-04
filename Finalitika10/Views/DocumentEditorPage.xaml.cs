using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class DocumentEditorPage : ContentPage
{
	public DocumentEditorPage(DocumentEditorViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}