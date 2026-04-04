using Finalitika10.ViewModels;
using Finalitika10.Views;

namespace Finalitika10;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }


}