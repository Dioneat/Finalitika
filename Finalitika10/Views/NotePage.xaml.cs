using Finalitika10.ViewModels.PlanViewModels;

namespace Finalitika10.Views;

public partial class NotePage : ContentPage
{
	public NotePage(NoteViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}