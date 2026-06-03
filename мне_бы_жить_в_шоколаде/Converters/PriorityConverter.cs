using System.Globalization;
using System.Windows.Data;

namespace мне_бы_жить_в_шоколаде.Converters;

public class PriorityConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string priority)
        {
            return priority switch
            {
                "low" => "Низкий",
                "medium" => "Средний",
                "high" => "Высокий",
                "critical" => "Критический",
                _ => string.IsNullOrWhiteSpace(priority) ? "Не указан" : priority
            };
        }
        throw new ArgumentException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}