using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Models;
using PPS.Services;

namespace PPS.ViewModels;

public partial class EmployeeDetailViewModel : BaseViewModel
{
    private readonly IEmployeeService _svc;

    public override string Title => IsNew ? "Add Employee" : "Edit Employee";

    // ── Fields ────────────────────────────────────────────────────────────────
    [ObservableProperty] private int         _employeeId;
    [ObservableProperty] private string      _employeeCode  = string.Empty;
    [ObservableProperty] private string      _firstName     = string.Empty;
    [ObservableProperty] private string      _lastName      = string.Empty;
    [ObservableProperty] private string      _middleName    = string.Empty;
    [ObservableProperty] private string      _department    = string.Empty;
    [ObservableProperty] private string      _position      = string.Empty;
    [ObservableProperty] private EmployeeType _employeeType = EmployeeType.NonTeaching;
    [ObservableProperty] private PayrollType  _payrollType  = PayrollType.Monthly;
    [ObservableProperty] private decimal     _basicMonthlySalary;
    [ObservableProperty] private decimal     _hourlyRate;
    [ObservableProperty] private string      _sSSNumber     = string.Empty;
    [ObservableProperty] private string      _tINNumber     = string.Empty;
    [ObservableProperty] private string      _philHealthNumber = string.Empty;
    [ObservableProperty] private string      _pagIBIGNumber = string.Empty;
    [ObservableProperty] private string      _biometricId   = string.Empty;
    [ObservableProperty] private DateOnly    _dateHired     = DateOnly.FromDateTime(DateTime.Today);
    [ObservableProperty] private bool        _isActive      = true;

    public bool IsNew => EmployeeId == 0;

    public IEnumerable<EmployeeType> EmployeeTypes => Enum.GetValues<EmployeeType>();
    public IEnumerable<PayrollType>  PayrollTypes  => Enum.GetValues<PayrollType>();

    public event Action? SaveSucceeded;

    public EmployeeDetailViewModel(IEmployeeService svc) => _svc = svc;

    /// <summary>Populate form from an existing employee (edit mode).</summary>
    public void LoadEmployee(Employee emp)
    {
        EmployeeId          = emp.EmployeeId;
        EmployeeCode        = emp.EmployeeCode;
        FirstName           = emp.FirstName;
        LastName            = emp.LastName;
        MiddleName          = emp.MiddleName ?? string.Empty;
        Department          = emp.Department;
        Position            = emp.Position;
        EmployeeType        = emp.EmployeeType;
        PayrollType         = emp.PayrollType;
        BasicMonthlySalary  = emp.BasicMonthlySalary;
        HourlyRate          = emp.HourlyRate;
        SSSNumber           = emp.SSSNumber ?? string.Empty;
        TINNumber           = emp.TINNumber ?? string.Empty;
        PhilHealthNumber    = emp.PhilHealthNumber ?? string.Empty;
        PagIBIGNumber       = emp.PagIBIGNumber ?? string.Empty;
        BiometricId         = emp.BiometricId ?? string.Empty;
        DateHired           = emp.DateHired;
        IsActive            = emp.IsActive;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!Validate()) return;
        IsBusy = true;
        ClearMessages();
        try
        {
            if (await _svc.EmployeeCodeExistsAsync(EmployeeCode.Trim(), IsNew ? null : EmployeeId))
            {
                SetError("Employee code already exists.");
                return;
            }

            var emp = new Employee
            {
                EmployeeId         = EmployeeId,
                EmployeeCode       = EmployeeCode.Trim(),
                FirstName          = FirstName.Trim(),
                LastName           = LastName.Trim(),
                MiddleName         = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
                Department         = Department.Trim(),
                Position           = Position.Trim(),
                EmployeeType       = EmployeeType,
                PayrollType        = PayrollType,
                BasicMonthlySalary = BasicMonthlySalary,
                HourlyRate         = HourlyRate,
                SSSNumber          = NullIfEmpty(SSSNumber),
                TINNumber          = NullIfEmpty(TINNumber),
                PhilHealthNumber   = NullIfEmpty(PhilHealthNumber),
                PagIBIGNumber      = NullIfEmpty(PagIBIGNumber),
                BiometricId        = NullIfEmpty(BiometricId),
                DateHired          = DateHired,
                IsActive           = IsActive,
            };

            if (IsNew) await _svc.AddAsync(emp);
            else       await _svc.UpdateAsync(emp);

            SetSuccess("Employee saved successfully.");
            SaveSucceeded?.Invoke();
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(EmployeeCode)) { SetError("Employee code is required."); return false; }
        if (string.IsNullOrWhiteSpace(FirstName))    { SetError("First name is required.");    return false; }
        if (string.IsNullOrWhiteSpace(LastName))     { SetError("Last name is required.");     return false; }
        if (string.IsNullOrWhiteSpace(Department))   { SetError("Department is required.");    return false; }
        if (BasicMonthlySalary <= 0 && PayrollType == PayrollType.Monthly)
        { SetError("Basic salary must be greater than zero."); return false; }
        if (HourlyRate <= 0 && PayrollType == PayrollType.HourlyUnit)
        { SetError("Hourly rate must be greater than zero."); return false; }
        return true;
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
