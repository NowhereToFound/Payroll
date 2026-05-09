using System.Windows;
using System.Windows.Controls;
using PPS.ViewModels;

namespace PPS.Views.Pages;

public partial class EmployeesPage : UserControl
{
    public EmployeesPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is EmployeeListViewModel vm)
            {
                vm.EditRequested += OnEditRequested;
                await vm.LoadCommand.ExecuteAsync(null);
            }
        };
    }

    private void OnEditRequested(Models.Employee? employee)
    {
        var detailVm = App.GetService<EmployeeDetailViewModel>();
        if (employee is not null) detailVm.LoadEmployee(employee);

        var dialog = new EmployeeDetailDialog(detailVm) { Owner = Window.GetWindow(this) };
        detailVm.SaveSucceeded += async () =>
        {
            dialog.Close();
            if (DataContext is EmployeeListViewModel listVm)
                await listVm.LoadCommand.ExecuteAsync(null);
        };
        dialog.ShowDialog();
    }
}
