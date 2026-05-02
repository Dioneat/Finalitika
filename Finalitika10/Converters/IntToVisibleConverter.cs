using System.Globalization;

namespace Finalitika10.Converters
{
    // Показывает элемент, только если CurrentStep совпадает с параметром
    public class IntToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentStep && parameter is string stepParam)
            {
                return currentStep == int.Parse(stepParam);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    // Меняет цвет точек прогресса
    public class StepToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentStep && parameter is string stepParam)
            {
                return currentStep == int.Parse(stepParam) ? Color.FromArgb("#3498DB") : Color.FromArgb("#D5D8DC");
            }
            return Color.FromArgb("#D5D8DC");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}