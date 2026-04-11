using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class PersonalDataPage : ContentPage
{
    public PersonalDataPage(PersonalDataViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}