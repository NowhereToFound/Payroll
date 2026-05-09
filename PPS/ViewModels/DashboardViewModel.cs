using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Services;

namespace PPS.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IEmployeeService _employees;
    private readonly IPayrollService  _payroll;
    private readonly ILoanService     _loans;

    public override string Title => "Dashboard";

    [ObservableProperty] private int     _totalEmployees;
    [ObservableProperty] private int     _teachingCount;
    [ObservableProperty] private int     _nonTeachingCount;
    [ObservableProperty] private decimal _currentPeriodNetPay;
    [ObservableProperty] private int     _activeLoansCount;
    [ObservableProperty] private string  _currentPeriodLabel = string.Empty;

    public DashboardViewModel(IEmployeeService employees, IPayrollService payroll, ILoanService loans)
    {
        _employees = employees;
        _payroll   = payroll;
        _loans     = loans;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            TotalEmployees    = await _employees.GetActiveCountAsync();
            TeachingCount     = await _employees.GetTeachingCountAsync();
            NonTeachingCount  = await _employees.GetNonTeachingCountAsync();

            var today  = DateTime.Today;
            var period = today.Day <= 15 ? Models.PayPeriod.FirstHalf : Models.PayPeriod.SecondHalf;
            CurrentPeriodLabel   = $"{today:MMMM yyyy} — {(period == Models.PayPeriod.FirstHalf ? "1st Half" : "2nd Half")}";
            CurrentPeriodNetPay  = await _payroll.GetPeriodTotalNetPayAsync(today.Year, today.Month, period);

            var allLoans     = await _loans.GetAllActiveLoansAsync();
            ActiveLoansCount = allLoans.Count();
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
