using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PPS.Models;
using PPS.Services;

namespace PPS.ViewModels;

public partial class AttendanceViewModel : BaseViewModel
{
    private readonly IAttendanceService _attendance;
    private readonly IEmployeeService   _employees;

    public override string Title => "Attendance";

    [ObservableProperty] private ObservableCollection<Employee>        _employeeList  = [];
    [ObservableProperty] private ObservableCollection<AttendanceRecord> _records      = [];
    [ObservableProperty] private Employee?  _selectedEmployee;
    [ObservableProperty] private int        _selectedYear   = DateTime.Today.Year;
    [ObservableProperty] private int        _selectedMonth  = DateTime.Today.Month;
    [ObservableProperty] private Models.PayPeriod _selectedPeriod = DateTime.Today.Day <= 15
        ? Models.PayPeriod.FirstHalf : Models.PayPeriod.SecondHalf;

    [ObservableProperty] private string _importStatusMessage = string.Empty;
    [ObservableProperty] private int    _importedCount;
    [ObservableProperty] private int    _skippedCount;

    public IEnumerable<int>               Years   => Enumerable.Range(2020, 20);
    public IEnumerable<int>               Months  => Enumerable.Range(1, 12);
    public IEnumerable<Models.PayPeriod>  Periods => Enum.GetValues<Models.PayPeriod>();

    public AttendanceViewModel(IAttendanceService attendance, IEmployeeService employees)
    {
        _attendance = attendance;
        _employees  = employees;
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
                await LoadRecordsAsync();
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task LoadRecordsAsync()
    {
        if (SelectedEmployee is null) return;
        IsBusy = true;
        ClearMessages();
        try
        {
            var (start, end) = GetPeriodDates();
            var recs = await _attendance.GetByPeriodAsync(SelectedEmployee.EmployeeId, start, end);
            Records = new ObservableCollection<AttendanceRecord>(recs);
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ImportCsvAsync()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Select Biometric Export CSV",
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
        };

        if (dlg.ShowDialog() != true) return;

        IsBusy = true;
        ImportStatusMessage = "Importing…";
        ClearMessages();

        try
        {
            var result = await _attendance.ImportFromCsvAsync(dlg.FileName);
            ImportedCount = result.Imported;
            SkippedCount  = result.Skipped;

            if (result.Errors.Count > 0)
                SetError($"Imported {result.Imported} rows. {result.Skipped} skipped.\n" +
                         string.Join("\n", result.Errors.Take(5)));
            else
                SetSuccess($"Successfully imported {result.Imported} attendance record(s).");

            await LoadRecordsAsync();
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally
        {
            IsBusy              = false;
            ImportStatusMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveRecordAsync(AttendanceRecord record)
    {
        try { await _attendance.UpsertAsync(record); SetSuccess("Record saved."); }
        catch (Exception ex) { SetError(ex.Message); }
    }

    partial void OnSelectedEmployeeChanged(Employee? value) => _ = LoadRecordsAsync();
    partial void OnSelectedYearChanged(int value)           => _ = LoadRecordsAsync();
    partial void OnSelectedMonthChanged(int value)          => _ = LoadRecordsAsync();
    partial void OnSelectedPeriodChanged(Models.PayPeriod value) => _ = LoadRecordsAsync();

    private (DateOnly Start, DateOnly End) GetPeriodDates()
        => SelectedPeriod == Models.PayPeriod.FirstHalf
            ? (new DateOnly(SelectedYear, SelectedMonth, 1),  new DateOnly(SelectedYear, SelectedMonth, 15))
            : (new DateOnly(SelectedYear, SelectedMonth, 16),
               new DateOnly(SelectedYear, SelectedMonth, DateTime.DaysInMonth(SelectedYear, SelectedMonth)));
}
