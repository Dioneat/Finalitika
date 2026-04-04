using Finalitika10.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using System.Text.Json;

namespace Finalitika10.Services.PlanServices
{
    public interface IPaymentService
    {
        List<PaymentReminder> GetAllPayments();
        void SavePayment(PaymentReminder payment);
        void DeletePayment(string id);
    }

    public class PaymentService : IPaymentService
    {
        private const string PaymentsKey = "UserPayments";

        public List<PaymentReminder> GetAllPayments()
        {
            var json = Preferences.Default.Get(PaymentsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return new List<PaymentReminder>();
            return JsonSerializer.Deserialize<List<PaymentReminder>>(json) ?? new List<PaymentReminder>();
        }

        public void SavePayment(PaymentReminder payment)
        {
            var payments = GetAllPayments();
            var existing = payments.FirstOrDefault(p => p.Id == payment.Id);

            if (existing != null) payments[payments.IndexOf(existing)] = payment;
            else payments.Add(payment);

            Preferences.Default.Set(PaymentsKey, JsonSerializer.Serialize(payments));

            UpdateNotification(payment);
        }

        public void DeletePayment(string id)
        {
            var payments = GetAllPayments();
            var payment = payments.FirstOrDefault(p => p.Id == id);
            if (payment != null)
            {
                payments.Remove(payment);
                Preferences.Default.Set(PaymentsKey, JsonSerializer.Serialize(payments));

                LocalNotificationCenter.Current.Cancel(payment.NotificationId);
            }
        }

        private void UpdateNotification(PaymentReminder payment)
        {
            if (!payment.IsPushEnabled)
            {
                LocalNotificationCenter.Current.Cancel(payment.NotificationId);
                return;
            }

            DateTime notifyTime;
            NotificationRepeat repeatType = NotificationRepeat.No;
            TimeSpan? repeatInterval = null;

            if (payment.ReminderType == "Ежемесячный")
            {
                var now = DateTime.Now;
                notifyTime = new DateTime(now.Year, now.Month, payment.MonthlyDay, payment.NotifyTime.Hours, payment.NotifyTime.Minutes, 0);

                if (notifyTime < now) notifyTime = notifyTime.AddMonths(1);

                repeatType = NotificationRepeat.TimeInterval;
                repeatInterval = TimeSpan.FromDays(30);
            }
            else
            {
                notifyTime = new DateTime(payment.ExactDate.Year, payment.ExactDate.Month, payment.ExactDate.Day, payment.NotifyTime.Hours, payment.NotifyTime.Minutes, 0);

                if (notifyTime < DateTime.Now) return;
            }

            var request = new NotificationRequest
            {
                NotificationId = payment.NotificationId,
                Title = "Напоминание о платеже 💸",
                Description = $"{payment.Title}: нужно оплатить {payment.Amount:N0} ₽",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime,
                    RepeatType = repeatType,
                    NotifyRepeatInterval = repeatInterval
                }
            };

            LocalNotificationCenter.Current.Show(request);
        }
    }
}
