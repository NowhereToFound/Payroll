using System.Windows.Controls;
using PPS.ViewModels;

namespace PPS.Views.Pages;

public partial class SystemLogPage : UserControl
{
    public SystemLogPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is SystemLogViewModel vm)
                await vm.LoadAsync();
        };
    }
}
