using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Models;
using PPS.Services;

namespace PPS.ViewModels;

public partial class SystemLogViewModel : BaseViewModel
{
    private readonly ISystemLogService _logs;

    public override string Title => "System Logs";

    // ── Filter state ──────────────────────────────────────────────────────────
    [ObservableProperty] private DateTime _fromDate        = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime _toDate          = DateTime.Today;
    [ObservableProperty] private string   _filterText      = string.Empty;
    [ObservableProperty] private string   _selectedSeverity = "All";

    // ── Data ──────────────────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<SystemLog> _entries = [];

    // ── Summary counts ────────────────────────────────────────────────────────
    public int TotalCount   => Entries.Count;
    public int InfoCount    => Entries.Count(e => e.Severity == LogSeverity.Info);
    public int WarningCount => Entries.Count(e => e.Severity == LogSeverity.Warning);
    public int ErrorCount   => Entries.Count(e => e.Severity == LogSeverity.Error);

    public IEnumerable<string> SeverityOptions => ["All", "Info", "Warning", "Error"];

    public SystemLogViewModel(ISystemLogService logs) => _logs = logs;

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var to  = ToDate.Date.AddDays(1).AddTicks(-1);
            var all = await _logs.GetByDateRangeAsync(FromDate.Date, to);

            var filtered = all.Where(l =>
                (SelectedSeverity == "All" || l.Severity.ToString() == SelectedSeverity) &&
                (string.IsNullOrWhiteSpace(FilterText) ||
                 l.Action.Contains(FilterText,   StringComparison.OrdinalIgnoreCase) ||
                 l.Module.Contains(FilterText,   StringComparison.OrdinalIgnoreCase) ||
                 l.Username.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                 (l.Details?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false)));

            Entries = new ObservableCollection<SystemLog>(filtered);
            RefreshSummaryCounts();
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void ApplyFilter() => _ = LoadAsync();

    // ── Reactive filter triggers ──────────────────────────────────────────────
    partial void OnSelectedSeverityChanged(string value) => _ = LoadAsync();
    partial void OnFromDateChanged(DateTime value)       => _ = LoadAsync();
    partial void OnToDateChanged(DateTime value)         => _ = LoadAsync();

    private void RefreshSummaryCounts()
    {
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(InfoCount));
        OnPropertyChanged(nameof(WarningCount));
        OnPropertyChanged(nameof(ErrorCount));
    }
}
