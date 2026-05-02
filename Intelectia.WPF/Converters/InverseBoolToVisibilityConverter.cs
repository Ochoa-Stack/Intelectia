using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Intelectia.WPF.Converters;

public class InverseBoolToVisibilityConverter : IValueConverter
{
    // Muestra el elemento cuando el valor es false; inverso de BoolToVisibilityConverter
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Collapsed;
}
