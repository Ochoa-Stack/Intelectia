using System.Windows;
using System.Windows.Media.Animation;

namespace Intelectia.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += (s, e) =>
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(vm.CurrentViewModel))
                    {
                        var fadeIn = new DoubleAnimation
                        {
                            From     = 0,
                            To       = 1,
                            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };
                        NavigationFrame.BeginAnimation(OpacityProperty, fadeIn);
                    }
                };
            }
        };
    }
}
