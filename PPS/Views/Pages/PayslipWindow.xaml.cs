using System.Windows;
using PPS.ViewModels;

namespace PPS.Views.Pages;

public partial class PayslipWindow : Window
{
    public PayslipWindow(PayslipViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
