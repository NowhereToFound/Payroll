using System.Windows;
using PPS.ViewModels;

namespace PPS.Views.Pages;

public partial class EmployeeDetailDialog : Window
{
    public EmployeeDetailDialog(EmployeeDetailViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
