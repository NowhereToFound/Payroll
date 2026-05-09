using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Models;
using PPS.Services;

namespace PPS.ViewModels;

public partial class EmployeeListViewModel : BaseViewModel
{
    private readonly IEmployeeService _svc;

    public override string Title => "Employees";

    [ObservableProperty] private ObservableCollection<Employee> _employees = [];
    [ObservableProperty] private Employee?  _selectedEmployee;
    [ObservableProperty] private string     _searchText = string.Empty;
    [ObservableProperty] private bool       _showInactive;

    /// <summary>Raised to open the Add/Edit dialog.</summary>
    public event Action<Employee?>? EditRequested;

    public EmployeeListViewModel(IEmployeeService svc) => _svc = svc;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var all = ShowInactive
                ? await _svc.GetAllAsync()
                : await _svc.GetAllActiveAsync();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? all
                : all.Where(e =>
                    e.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || e.EmployeeCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || e.Department.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            Employees = new ObservableCollection<Employee>(filtered);
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void AddEmployee() => EditRequested?.Invoke(null);

    [RelayCommand]
    private void EditEmployee(Employee? emp) => EditRequested?.Invoke(emp ?? SelectedEmployee);

    [RelayCommand]
    private async Task DeactivateEmployeeAsync(Employee? emp)
    {
        var target = emp ?? SelectedEmployee;
        if (target is null) return;

        try
        {
            await _svc.DeactivateAsync(target.EmployeeId);
            SetSuccess($"{target.FullName} has been deactivated.");
            await LoadAsync();
        }
        catch (Exception ex) { SetError(ex.Message); }
    }

    partial void OnSearchTextChanged(string value) => _ = LoadAsync();
    partial void OnShowInactiveChanged(bool value) => _ = LoadAsync();
}
