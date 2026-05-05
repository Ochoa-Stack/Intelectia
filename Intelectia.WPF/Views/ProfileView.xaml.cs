using System.Windows.Controls;
using Intelectia.WPF.ViewModels;

namespace Intelectia.WPF.Views;

public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
    }

    private void CurrentPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel vm)
            vm.CurrentPassword = CurrentPasswordBox.Password;
    }

    private void NewPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel vm)
            vm.NewPassword = NewPasswordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel vm)
            vm.ConfirmNewPassword = ConfirmPasswordBox.Password;
    }
}
