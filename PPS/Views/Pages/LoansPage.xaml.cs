using System.Windows.Controls;
using PPS.ViewModels;

namespace PPS.Views.Pages;

public partial class LoansPage : UserControl
{
    public LoansPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is LoanViewModel vm)
                await vm.InitializeCommand.ExecuteAsync(null);
        };
    }
}
