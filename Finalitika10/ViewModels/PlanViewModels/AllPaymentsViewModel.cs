using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services.PlanServices;
using Finalitika10.Views.PlanPages;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.PlanViewModels
{
    public partial class AllPaymentsViewModel : ObservableObject
    {
        private readonly IPaymentService _paymentService;
        public ObservableCollection<PaymentReminder> Payments { get; } = new();

        public AllPaymentsViewModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
            LoadPayments();

            WeakReferenceMessenger.Default.Register<PaymentAddedMessage>(this, (r, m) => LoadPayments());
        }

        private void LoadPayments()
        {
            Payments.Clear();
            var all = _paymentService.GetAllPayments().OrderBy(p => p.MonthlyDay).ToList();
            foreach (var p in all) Payments.Add(p);
        }

        [RelayCommand]
        private async Task AddPaymentAsync()
        {
            await Shell.Current.Navigation.PushModalAsync(new AddPaymentPage(new AddPaymentViewModel(_paymentService, null)));
        }

        [RelayCommand]
        private async Task EditPaymentAsync(PaymentReminder payment)
        {
            if (payment == null) return;
            await Shell.Current.Navigation.PushModalAsync(new AddPaymentPage(new AddPaymentViewModel(_paymentService, payment)));
        }

        [RelayCommand]
        private async Task DeletePaymentAsync(PaymentReminder payment)
        {
            if (payment == null) return;

            bool answer = await App.Current.MainPage.DisplayAlertAsync("Удаление", $"Удалить напоминание '{payment.Title}'?", "Да", "Отмена");
            if (answer)
            {
                _paymentService.DeletePayment(payment.Id);
                Payments.Remove(payment);

                WeakReferenceMessenger.Default.Send(new PaymentAddedMessage(null));
            }
        }
    }
}
