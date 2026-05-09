using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Models;

namespace PPS.ViewModels;

public partial class PayslipViewModel : BaseViewModel
{
    public override string Title => "Payslip";

    [ObservableProperty] private PayrollRecord? _record;
    [ObservableProperty] private string         _companyName = "St. Dominic Savio College";
    [ObservableProperty] private string         _companyAddress = "Philippines";

    public event Action<PayrollRecord>? PrintRequested;

    public void LoadRecord(PayrollRecord record) => Record = record;

    [RelayCommand]
    private void Print()
    {
        if (Record is not null) PrintRequested?.Invoke(Record);
    }
}
