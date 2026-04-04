using Finalitika10.Models;
using System.Globalization;

namespace Finalitika10.Converters
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                ProjectType.Split => "Сплит",
                ProjectType.PersonalGoal => "Личная цель",
                TargetCalculationType.FixedAmount => "Фиксированная сумма",
                TargetCalculationType.ExpenseList => "По списку расходов",
                _ => value?.ToString() ?? string.Empty
            };
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}