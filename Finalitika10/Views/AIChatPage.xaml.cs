using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class AIChatPage : ContentPage
{
	public AIChatPage(AIChatViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}