using System.Globalization;
using System.Windows.Data;

namespace мне_бы_жить_в_шоколаде.Converters;

public class EquipmentStatusConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status switch
            {
                "repair" => "Ремонтируется",
                "broken" => "Сломан",
                "in_use" => "Используется",
                "in_stock" => "На складе",
                _ => string.IsNullOrWhiteSpace(status) ? "Не указан" : status
            };
        }
        throw new ArgumentException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}