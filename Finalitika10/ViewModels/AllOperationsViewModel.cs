using CommunityToolkit.Mvvm.ComponentModel;
using Finalitika10.Models;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels
{
    public partial class AllOperationsViewModel : ObservableObject
    {
        public ObservableCollection<OperationData> Operations { get; } = new();

        public AllOperationsViewModel()
        {
            LoadOperations();
        }

        private void LoadOperations()
        {
            Operations.Add(new OperationData { Date = "15.03.2026", Type = "Покупка", Asset = "Apple Inc.", Amount = "-125 000 ₽", Status = "Выполнено" });
            Operations.Add(new OperationData { Date = "12.03.2026", Type = "Продажа", Asset = "Газпром", Amount = "+45 000 ₽", Status = "Выполнено" });
            Operations.Add(new OperationData { Date = "10.03.2026", Type = "Дивиденды", Asset = "Сбербанк", Amount = "+12 500 ₽", Status = "Зачислено" });
            Operations.Add(new OperationData { Date = "05.03.2026", Type = "Пополнение", Asset = "Брокерский счет", Amount = "+200 000 ₽", Status = "Зачислено" });
        }
    }
}
