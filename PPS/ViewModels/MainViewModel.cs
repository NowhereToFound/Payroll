using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Services;

namespace PPS.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly NavigationService  _nav;
    private readonly CurrentUserService _session;

    // Injected page VMs — resolved from DI
    private readonly DashboardViewModel      _dashboard;
    private readonly EmployeeListViewModel   _employees;
    private readonly AttendanceViewModel     _attendance;
    private readonly PayrollProcessViewModel _payroll;
    private readonly LoanViewModel           _loans;
    private readonly ReportsViewModel        _reports;
    private readonly SystemLogViewModel      _systemLog;

    [ObservableProperty] private BaseViewModel _currentViewModel;
    [ObservableProperty] private string        _currentUserName = string.Empty;
    [ObservableProperty] private string        _currentUserRole = string.Empty;
    [ObservableProperty] private bool          _isDashboardSelected  = true;
    [ObservableProperty] private bool          _isEmployeesSelected;
    [ObservableProperty] private bool          _isAttendanceSelected;
    [ObservableProperty] private bool          _isPayrollSelected;
    [ObservableProperty] private bool          _isLoansSelected;
    [ObservableProperty] private bool          _isReportsSelected;
    [ObservableProperty] private bool          _isSystemLogSelected;

    public event Action? LogoutRequested;

    public MainViewModel(
        NavigationService nav, CurrentUserService session,
        DashboardViewModel dashboard, EmployeeListViewModel employees,
        AttendanceViewModel attendance, PayrollProcessViewModel payroll,
        LoanViewModel loans, ReportsViewModel reports,
        SystemLogViewModel systemLog)
    {
        _nav        = nav;
        _session    = session;
        _dashboard  = dashboard;
        _employees  = employees;
        _attendance = attendance;
        _payroll    = payroll;
        _loans      = loans;
        _reports    = reports;
        _systemLog  = systemLog;

        _currentViewModel = dashboard;

        if (session.CurrentUser is not null)
        {
            CurrentUserName = session.CurrentUser.FullName;
            CurrentUserRole = session.CurrentUser.Role.ToString();
        }
    }

    [RelayCommand] private void GoToDashboard()  => Select(_dashboard,  nameof(IsDashboardSelected));
    [RelayCommand] private void GoToEmployees()  => Select(_employees,  nameof(IsEmployeesSelected));
    [RelayCommand] private void GoToAttendance() => Select(_attendance, nameof(IsAttendanceSelected));
    [RelayCommand] private void GoToPayroll()    => Select(_payroll,    nameof(IsPayrollSelected));
    [RelayCommand] private void GoToLoans()      => Select(_loans,      nameof(IsLoansSelected));
    [RelayCommand] private void GoToReports()    => Select(_reports,    nameof(IsReportsSelected));
    [RelayCommand] private void GoToSystemLog()  => Select(_systemLog,  nameof(IsSystemLogSelected));

    [RelayCommand]
    private void Logout()
    {
        _session.ClearUser();
        LogoutRequested?.Invoke();
    }

    private void Select(BaseViewModel vm, string selectedPropertyName)
    {
        IsDashboardSelected  = selectedPropertyName == nameof(IsDashboardSelected);
        IsEmployeesSelected  = selectedPropertyName == nameof(IsEmployeesSelected);
        IsAttendanceSelected = selectedPropertyName == nameof(IsAttendanceSelected);
        IsPayrollSelected    = selectedPropertyName == nameof(IsPayrollSelected);
        IsLoansSelected      = selectedPropertyName == nameof(IsLoansSelected);
        IsReportsSelected    = selectedPropertyName == nameof(IsReportsSelected);
        IsSystemLogSelected  = selectedPropertyName == nameof(IsSystemLogSelected);
        CurrentViewModel     = vm;
    }
}
