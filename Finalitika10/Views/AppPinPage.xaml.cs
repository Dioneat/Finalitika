using Finalitika10.ViewModels;

namespace Finalitika10.Views;

public partial class AppPinPage : ContentPage
{
    public AppPinPage()
    {
        InitializeComponent();
        BindingContext = new AppPinViewModel();
    }

    // Блокируем системную кнопку "Назад" на Android, чтобы нельзя было обойти ПИН-код
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}