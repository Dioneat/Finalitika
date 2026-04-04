

using Finalitika10.ViewModels.AnalysisViewModels;

namespace Finalitika10;

public partial class FinancialAnalysisPage : ContentPage
{
    public FinancialAnalysisPage(AnalysisViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;


    }
}