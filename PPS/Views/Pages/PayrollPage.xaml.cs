using System.Windows.Controls;
using PPS.ViewModels;
using PPS.Views.Pages;

namespace PPS.Views.Pages;

public partial class PayrollPage : UserControl
{
    public PayrollPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is PayrollProcessViewModel vm)
            {
                vm.ViewPayslipRequested += OnViewPayslip;
                await vm.LoadCommand.ExecuteAsync(null);
            }
        };
    }

    private void OnViewPayslip(Models.PayrollRecord record)
    {
        var payslipVm = App.GetService<PayslipViewModel>();
        payslipVm.LoadRecord(record);
        var win = new PayslipWindow(payslipVm)
        {
            Owner = System.Windows.Window.GetWindow(this)
        };
        win.ShowDialog();
    }
}
