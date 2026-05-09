using System.Windows.Controls;
using Intelectia.WPF.ViewModels.Auth;

namespace Intelectia.WPF.Views.Auth;

public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();

        // Nos suscribimos al cambio de DataContext para re-enganchar los eventos
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        // Limpiamos los PasswordBox cuando cambia el ViewModel
        // para evitar que queden valores residuales de sesiones anteriores
        PasswordBox.Clear();
        ConfirmPasswordBox.Clear();
    }

    private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        // Propagamos el valor inmediatamente al ViewModel
        if (DataContext is RegisterViewModel vm)
            vm.Password = PasswordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        // Propagamos el valor inmediatamente al ViewModel
        if (DataContext is RegisterViewModel vm)
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
    }
}
