using System.Windows;
using System.Windows.Controls;

namespace Intelectia.WPF.Controls;

public partial class IntelectiaLogo : UserControl
{
    // Tamaño del ícono en píxeles
    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(double),
            typeof(IntelectiaLogo), new PropertyMetadata(32.0));

    // Tamaño de la fuente del wordmark
    public static readonly new DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double),
            typeof(IntelectiaLogo), new PropertyMetadata(20.0));

    // Controla si se muestra el wordmark (Visible) o solo el ícono (Collapsed)
    public static readonly DependencyProperty ShowWordmarkProperty =
        DependencyProperty.Register(nameof(ShowWordmark), typeof(Visibility),
            typeof(IntelectiaLogo), new PropertyMetadata(Visibility.Visible));

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public new double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Visibility ShowWordmark
    {
        get => (Visibility)GetValue(ShowWordmarkProperty);
        set => SetValue(ShowWordmarkProperty, value);
    }

    public IntelectiaLogo()
    {
        InitializeComponent();
    }
}
