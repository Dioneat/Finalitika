using Finalitika10.ViewModels;
using Finalitika10.ViewModels.PlanViewModels;

namespace Finalitika10;

public partial class PlansPage : ContentPage
{

    public PlansPage(PlansViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}