using System.Windows;
using System.Windows.Input;
using PPS.ViewModels;

namespace PPS.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        _vm          = vm;
        DataContext  = vm;
        vm.LoginSucceeded += OnLoginSucceeded;

        // PasswordBox doesn't support {Binding} — sync manually on every keystroke
        PasswordBox.PasswordChanged += (_, _) => _vm.Password = PasswordBox.Password;
    }

    private void OnLoginSucceeded(Models.User user)
    {
        var shell = App.GetService<ShellWindow>();
        shell.Show();
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) PasswordBox.Focus();
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        // Relay the password to the ViewModel (PasswordBox doesn't support binding by default)
        _vm.Password = PasswordBox.Password;
        if (_vm.LoginCommand.CanExecute(null))
            _vm.LoginCommand.Execute(null);
    }

    // Allow drag on the custom title-bar-less window
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        DragMove();
    }
}
