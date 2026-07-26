using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gamora.App.Converters;

// ConverterParameter="Invert" verilirse null iken Visible, doluyken Collapsed döner.
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isNull = value is null;
        var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);

        var visible = invert ? isNull : !isNull;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
