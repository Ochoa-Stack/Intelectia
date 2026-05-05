using System.Globalization;
using System.Windows.Data;

namespace Intelectia.WPF.Converters;

public class InverseBoolConverter : IValueConverter
{
    // Invierte un booleano; útil para IsEnabled="{Binding IsBusy, Converter=...}"
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;
}
