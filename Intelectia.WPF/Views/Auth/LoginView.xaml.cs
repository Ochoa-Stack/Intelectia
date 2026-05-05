using System.Windows.Controls;
using Intelectia.WPF.ViewModels.Auth;

namespace Intelectia.WPF.Views.Auth;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    // PasswordBox no soporta binding directo; lo pasamos al ViewModel manualmente
    private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }
}
