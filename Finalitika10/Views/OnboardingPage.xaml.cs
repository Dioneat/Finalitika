using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class OnboardingPage : ContentPage
{
	public OnboardingPage(OnboardingViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;	
	}
}