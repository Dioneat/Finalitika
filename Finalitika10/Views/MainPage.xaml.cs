using Finalitika10.ViewModels;

namespace Finalitika10
{
    public partial class MainPage : ContentPage
    {
        
        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

        }
       
       
    }
  
}
