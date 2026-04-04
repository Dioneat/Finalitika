using Finalitika10.ViewModels;

namespace Finalitika10;

public partial class InvestmentsPage : ContentPage
{
   
    public InvestmentsPage(InvestmentsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
 
   
}
