using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services.PlanServices;
using Plugin.LocalNotification;

namespace Finalitika10.ViewModels.PlanViewModels
{
    public record PaymentAddedMessage(PaymentReminder Reminder);

    public partial class AddPaymentViewModel : ObservableObject
    {
        private readonly IPaymentService _paymentService;

        [ObservableProperty] private PaymentReminder reminder;

        public AddPaymentViewModel(IPaymentService paymentService, PaymentReminder existingPayment)
        {
            _paymentService = paymentService;

            Reminder = existingPayment != null
                ? new PaymentReminder
                {
                    Id = existingPayment.Id,
                    Title = existingPayment.Title,
                    Amount = existingPayment.Amount,
                    ReminderType = existingPayment.ReminderType,
                    ExactDate = existingPayment.ExactDate,
                    MonthlyDay = existingPayment.MonthlyDay,
                    IsPushEnabled = existingPayment.IsPushEnabled,
                    NotifyTime = existingPayment.NotifyTime
                }
                : new PaymentReminder();
        }

        public List<string> ReminderTypes { get; } = new() { "Ежемесячный", "Разовый" };

        public List<int> DaysOfMonth { get; } = Enumerable.Range(1, 31).ToList();

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Reminder.Title))
            {
                await App.Current.MainPage.DisplayAlert("Ошибка", "Введите название платежа", "ОК");
                return;
            }

            if (Reminder.IsPushEnabled)
            {
                if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
                {
                    bool isGranted = await LocalNotificationCenter.Current.RequestNotificationPermission();

                    if (!isGranted)
                    {
                        Reminder.IsPushEnabled = false;
                        await Shell.Current.DisplayAlertAsync(
                            "Уведомления отключены",
                            "Без разрешения мы не сможем напоминать вам о платежах. Напоминание сохранено, но пуш-уведомление отключено.",
                            "Понятно");
                    }
                }
            }

            _paymentService.SavePayment(Reminder);

            WeakReferenceMessenger.Default.Send(new PaymentAddedMessage(Reminder));

            await Shell.Current.Navigation.PopModalAsync();
        }

        [RelayCommand]
        private async Task CloseAsync()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}