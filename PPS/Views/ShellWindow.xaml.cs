using System.Windows;
using System.Windows.Input;
using PPS.ViewModels;

namespace PPS.Views;

public partial class ShellWindow : Window
{
    private readonly MainViewModel _vm;

    public ShellWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        _vm         = vm;
        vm.LogoutRequested += OnLogoutRequested;
        Loaded += (_, _) =>
        {
            TodayLabel.Text = DateTime.Now.ToString("dddd, MMMM d, yyyy");
            vm.GoToDashboardCommand.Execute(null);
        };
    }

    private void OnLogoutRequested()
    {
        var login = App.GetService<LoginWindow>();
        login.Show();
        Close();
    }

    private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }
}

