using Finalitika10.ViewModels;
using Finalitika10.ViewModels.PlanViewModels;

namespace Finalitika10.Views;

public partial class SalaryCalculatorPage : ContentPage
{
	public SalaryCalculatorPage(SalaryCalculatorViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}