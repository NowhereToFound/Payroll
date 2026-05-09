using System.Windows.Controls;
using PPS.ViewModels;

namespace PPS.Views.Pages;

public partial class AttendancePage : UserControl
{
    public AttendancePage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is AttendanceViewModel vm)
                await vm.InitializeCommand.ExecuteAsync(null);
        };
    }
}
