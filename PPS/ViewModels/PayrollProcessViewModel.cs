using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Models;
using PPS.Services;

namespace PPS.ViewModels;

public partial class PayrollProcessViewModel : BaseViewModel
{
    private readonly IPayrollService  _payroll;
    private readonly IEmployeeService _employees;

    public override string Title => "Payroll Processing";

    [ObservableProperty] private int   _selectedYear  = DateTime.Today.Year;
    [ObservableProperty] private int   _selectedMonth = DateTime.Today.Month;
    [ObservableProperty] private PayPeriod _selectedPeriod = DateTime.Today.Day <= 15
        ? PayPeriod.FirstHalf : PayPeriod.SecondHalf;

    [ObservableProperty] private ObservableCollection<PayrollRecord> _records = [];
    [ObservableProperty] private PayrollRecord? _selectedRecord;
    [ObservableProperty] private decimal        _periodTotalNet;
    [ObservableProperty] private bool           _allPosted;

    public IEnumerable<int>       Years   => Enumerable.Range(2020, 20);
    public IEnumerable<int>       Months  => Enumerable.Range(1, 12);
    public IEnumerable<PayPeriod> Periods => Enum.GetValues<PayPeriod>();

    public event Action<PayrollRecord>? ViewPayslipRequested;

    public PayrollProcessViewModel(IPayrollService payroll, IEmployeeService employees)
    {
        _payroll   = payroll;
        _employees = employees;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var recs    = await _payroll.GetByPeriodAsync(SelectedYear, SelectedMonth, SelectedPeriod);
            Records     = new ObservableCollection<PayrollRecord>(recs);
            PeriodTotalNet = Records.Sum(r => r.NetPay);
            AllPosted      = Records.Count > 0 && Records.All(r => r.IsPosted);
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RunPayrollForAllAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            await _payroll.ComputeAllAsync(SelectedYear, SelectedMonth, SelectedPeriod);
            SetSuccess("Payroll computed for all active employees.");
            await LoadAsync();
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task PostAllAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            foreach (var rec in Records.Where(r => !r.IsPosted))
                await _payroll.PostAsync(rec.PayrollId);
            SetSuccess("All payroll records posted successfully.");
            await LoadAsync();
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task PostSingleAsync(PayrollRecord? rec)
    {
        if (rec is null) return;
        try
        {
            await _payroll.PostAsync(rec.PayrollId);
            SetSuccess($"Payroll for {rec.Employee?.FullName} posted.");
            await LoadAsync();
        }
        catch (Exception ex) { SetError(ex.Message); }
    }

    [RelayCommand]
    private void ViewPayslip(PayrollRecord? rec)
    {
        if (rec is not null) ViewPayslipRequested?.Invoke(rec);
    }

    partial void OnSelectedYearChanged(int value)           { _ = LoadAsync(); }
    partial void OnSelectedMonthChanged(int value)          { _ = LoadAsync(); }
    partial void OnSelectedPeriodChanged(PayPeriod value)   { _ = LoadAsync(); }
}
