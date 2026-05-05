using System.Windows.Controls;
using System.Windows.Input;
using Intelectia.WPF.ViewModels;

namespace Intelectia.WPF.Views;

public partial class GroupChatView : UserControl
{
    public GroupChatView()
    {
        InitializeComponent();
    }

    // Notifica al ViewModel que el usuario está escribiendo
    private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is GroupChatViewModel vm)
            _ = vm.NotifyTypingCommand.ExecuteAsync(null);
    }

    // Enter envía el mensaje; 'Shift+Enter' agrega salto de línea
    private void MessageInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter &&
            e.KeyboardDevice.Modifiers != ModifierKeys.Shift &&
            DataContext is GroupChatViewModel vm)
        {
            e.Handled = true;
            _ = vm.SendMessageCommand.ExecuteAsync(null);
        }
    }
}
