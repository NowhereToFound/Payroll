using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Models;
using PPS.Services;

namespace PPS.ViewModels;

public partial class LoanViewModel : BaseViewModel
{
    private readonly ILoanService     _loans;
    private readonly IEmployeeService _employees;

    public override string Title => "Loans";

    [ObservableProperty] private ObservableCollection<Employee>   _employeeList = [];
    [ObservableProperty] private ObservableCollection<LoanRecord> _loanList     = [];
    [ObservableProperty] private Employee?   _selectedEmployee;
    [ObservableProperty] private LoanRecord? _selectedLoan;

    // ── New loan form fields ──────────────────────────────────────────────────
    [ObservableProperty] private LoanType _newLoanType        = LoanType.CompanyLoan;
    [ObservableProperty] private decimal  _newLoanAmount;
    [ObservableProperty] private decimal  _newMonthlyAmort;
    [ObservableProperty] private DateOnly _newStartDate       = DateOnly.FromDateTime(DateTime.Today);
    [ObservableProperty] private string   _newLoanRemarks     = string.Empty;
    [ObservableProperty] private bool     _isAddPanelVisible;

    // ── Manual payment form ───────────────────────────────────────────────────
    [ObservableProperty] private decimal  _manualPaymentAmount;
    [ObservableProperty] private bool     _isPaymentPanelVisible;

    public IEnumerable<LoanType> LoanTypes => Enum.GetValues<LoanType>();

    public LoanViewModel(ILoanService loans, IEmployeeService employees)
    {
        _loans     = loans;
        _employees = employees;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var emps   = await _employees.GetAllActiveAsync();
            EmployeeList = new ObservableCollection<Employee>(emps);
            if (EmployeeList.Count > 0)
            {
                SelectedEmployee = EmployeeList[0];
                await LoadLoansAsync();
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task LoadLoansAsync()
    {
        if (SelectedEmployee is null) return;
        IsBusy = true;
        ClearMessages();
        try
        {
            var items  = await _loans.GetAllLoansAsync(SelectedEmployee.EmployeeId);
            LoanList   = new ObservableCollection<LoanRecord>(items);
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void ShowAddPanel()
    {
        IsAddPanelVisible     = true;
        IsPaymentPanelVisible = false;
        NewLoanAmount         = 0;
        NewMonthlyAmort       = 0;
        NewLoanRemarks        = string.Empty;
        NewStartDate          = DateOnly.FromDateTime(DateTime.Today);
    }

    [RelayCommand]
    private void CancelAdd()
    {
        IsAddPanelVisible     = false;
        IsPaymentPanelVisible = false;
    }

    [RelayCommand]
    private async Task SaveLoanAsync()
    {
        if (SelectedEmployee is null) return;
        if (NewLoanAmount <= 0) { SetError("Loan amount must be > 0."); return; }
        if (NewMonthlyAmort <= 0) { SetError("Monthly amortization must be > 0."); return; }

        IsBusy = true;
        try
        {
            await _loans.AddLoanAsync(new LoanRecord
            {
                EmployeeId           = SelectedEmployee.EmployeeId,
                LoanType             = NewLoanType,
                LoanAmount           = NewLoanAmount,
                Balance              = NewLoanAmount,
                MonthlyAmortization  = NewMonthlyAmort,
                StartDate            = NewStartDate,
                Remarks              = string.IsNullOrWhiteSpace(NewLoanRemarks) ? null : NewLoanRemarks.Trim(),
            });
            SetSuccess("Loan added successfully.");
            IsAddPanelVisible = false;
            await LoadLoansAsync();
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void ShowPaymentPanel(LoanRecord? rec)
    {
        if (rec is null) return;
        SelectedLoan            = rec;
        ManualPaymentAmount     = rec.SemiMonthlyAmortization;
        IsPaymentPanelVisible   = true;
        IsAddPanelVisible       = false;
    }

    [RelayCommand]
    private async Task RecordPaymentAsync()
    {
        if (SelectedLoan is null || ManualPaymentAmount <= 0) return;
        IsBusy = true;
        try
        {
            await _loans.ProcessPaymentAsync(SelectedLoan.LoanId, ManualPaymentAmount,
                DateOnly.FromDateTime(DateTime.Today));
            SetSuccess($"Payment of ₱{ManualPaymentAmount:N2} recorded.");
            IsPaymentPanelVisible = false;
            await LoadLoansAsync();
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    partial void OnSelectedEmployeeChanged(Employee? value) => _ = LoadLoansAsync();
}
