using System.Windows.Controls;
using PPS.ViewModels;

namespace PPS.Views.Pages;

public partial class DashboardPage : UserControl
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is DashboardViewModel vm)
                await vm.LoadCommand.ExecuteAsync(null);
        };
    }
}
