using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPS.Services;

namespace PPS.ViewModels;

public enum ReportType
{
    PayrollSummary,
    SSSRemittance,
    PhilHealthRemittance,
    PagIBIGRemittance,
    LoanSummary,
    ThirteenthMonth
}

public partial class ReportsViewModel : BaseViewModel
{
    private readonly IPayrollService  _payroll;
    private readonly IEmployeeService _employees;
    private readonly ILoanService     _loans;

    public override string Title => "Reports";

    [ObservableProperty] private ReportType _selectedReportType = ReportType.PayrollSummary;
    [ObservableProperty] private int        _selectedYear       = DateTime.Today.Year;
    [ObservableProperty] private int        _selectedMonth      = DateTime.Today.Month;
    [ObservableProperty] private Models.PayPeriod _selectedPeriod = Models.PayPeriod.FirstHalf;
    [ObservableProperty] private string     _previewText        = string.Empty;

    public IEnumerable<ReportType>         ReportTypes => Enum.GetValues<ReportType>();
    public IEnumerable<int>                Years       => Enumerable.Range(2020, 20);
    public IEnumerable<int>                Months      => Enumerable.Range(1, 12);
    public IEnumerable<Models.PayPeriod>   Periods     => Enum.GetValues<Models.PayPeriod>();

    public ReportsViewModel(IPayrollService payroll, IEmployeeService employees, ILoanService loans)
    {
        _payroll   = payroll;
        _employees = employees;
        _loans     = loans;
    }

    [RelayCommand]
    private async Task GeneratePreviewAsync()
    {
        IsBusy = true;
        ClearMessages();
        PreviewText = string.Empty;
        try
        {
            var records = await _payroll.GetByPeriodAsync(SelectedYear, SelectedMonth, SelectedPeriod);
            var lines   = new System.Text.StringBuilder();

            lines.AppendLine($"ST. DOMINIC SAVIO COLLEGE — {SelectedReportType}");
            lines.AppendLine($"Period: {new DateTime(SelectedYear, SelectedMonth, 1):MMMM yyyy} — {SelectedPeriod}");
            lines.AppendLine(new string('─', 80));

            switch (SelectedReportType)
            {
                case ReportType.PayrollSummary:
                    lines.AppendLine($"{"Employee",-30} {"Gross",12} {"Deductions",12} {"Net Pay",12}");
                    lines.AppendLine(new string('─', 70));
                    foreach (var r in records)
                    {
                        decimal deductions = r.SSS_Employee + r.PhilHealth_Employee + r.PagIBIG_Employee
                                           + r.WithholdingTax + r.LoanDeductions;
                        lines.AppendLine($"{r.Employee?.FullName,-30} {r.GrossPay,12:N2} {deductions,12:N2} {r.NetPay,12:N2}");
                    }
                    lines.AppendLine(new string('─', 70));
                    lines.AppendLine($"{"TOTAL",-30} {records.Sum(r => r.GrossPay),12:N2} {records.Sum(r => r.NetPay),12:N2}");
                    break;

                case ReportType.SSSRemittance:
                    lines.AppendLine($"{"Employee",-30} {"EE Share",12} {"ER Share",12} {"Total",12}");
                    lines.AppendLine(new string('─', 70));
                    foreach (var r in records)
                        lines.AppendLine($"{r.Employee?.FullName,-30} {r.SSS_Employee,12:N2} {r.SSS_Employer,12:N2} {r.SSS_Employee + r.SSS_Employer,12:N2}");
                    lines.AppendLine($"{"TOTAL",-30} {records.Sum(r => r.SSS_Employee),12:N2} {records.Sum(r => r.SSS_Employer),12:N2} {records.Sum(r => r.SSS_Employee + r.SSS_Employer),12:N2}");
                    break;

                case ReportType.PhilHealthRemittance:
                    lines.AppendLine($"{"Employee",-30} {"EE Share",12} {"ER Share",12} {"Total",12}");
                    lines.AppendLine(new string('─', 70));
                    foreach (var r in records)
                        lines.AppendLine($"{r.Employee?.FullName,-30} {r.PhilHealth_Employee,12:N2} {r.PhilHealth_Employer,12:N2} {r.PhilHealth_Employee + r.PhilHealth_Employer,12:N2}");
                    break;

                case ReportType.PagIBIGRemittance:
                    lines.AppendLine($"{"Employee",-30} {"EE Share",12} {"ER Share",12}");
                    lines.AppendLine(new string('─', 70));
                    foreach (var r in records)
                        lines.AppendLine($"{r.Employee?.FullName,-30} {r.PagIBIG_Employee,12:N2} {r.PagIBIG_Employer,12:N2}");
                    break;
            }

            PreviewText = lines.ToString();
            if (string.IsNullOrWhiteSpace(PreviewText))
                PreviewText = "No data found for the selected period.";
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title            = "Export Report",
                Filter           = "Excel Files (*.xlsx)|*.xlsx",
                FileName         = $"SDSC_{SelectedReportType}_{SelectedYear}{SelectedMonth:D2}.xlsx"
            };
            if (dlg.ShowDialog() != true) return;

            var records = await _payroll.GetByPeriodAsync(SelectedYear, SelectedMonth, SelectedPeriod);

            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws      = wb.Worksheets.Add("Report");

            ws.Cell(1, 1).Value = "St. Dominic Savio College";
            ws.Cell(2, 1).Value = $"{SelectedReportType} — {new DateTime(SelectedYear, SelectedMonth, 1):MMMM yyyy} {SelectedPeriod}";
            ws.Cell(4, 1).Value = "Employee";
            ws.Cell(4, 2).Value = "Basic Pay";
            ws.Cell(4, 3).Value = "OT Pay";
            ws.Cell(4, 4).Value = "Gross Pay";
            ws.Cell(4, 5).Value = "SSS EE";
            ws.Cell(4, 6).Value = "PhilHealth EE";
            ws.Cell(4, 7).Value = "PagIBIG EE";
            ws.Cell(4, 8).Value = "Tax";
            ws.Cell(4, 9).Value = "Loans";
            ws.Cell(4, 10).Value = "Net Pay";

            int row = 5;
            foreach (var r in records)
            {
                ws.Cell(row, 1).Value  = r.Employee?.FullName;
                ws.Cell(row, 2).Value  = (double)r.BasicPay;
                ws.Cell(row, 3).Value  = (double)r.OvertimePay;
                ws.Cell(row, 4).Value  = (double)r.GrossPay;
                ws.Cell(row, 5).Value  = (double)r.SSS_Employee;
                ws.Cell(row, 6).Value  = (double)r.PhilHealth_Employee;
                ws.Cell(row, 7).Value  = (double)r.PagIBIG_Employee;
                ws.Cell(row, 8).Value  = (double)r.WithholdingTax;
                ws.Cell(row, 9).Value  = (double)r.LoanDeductions;
                ws.Cell(row, 10).Value = (double)r.NetPay;
                row++;
            }

            ws.Columns().AdjustToContents();
            wb.SaveAs(dlg.FileName);
            SetSuccess($"Report exported to {dlg.FileName}");
        }
        catch (Exception ex) { SetError(ex.Message); }
        finally { IsBusy = false; }
    }
}
