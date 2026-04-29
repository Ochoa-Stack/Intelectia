using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Intelectia.WPF.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    // Convierte true a Visible y false a Collapsed
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}
