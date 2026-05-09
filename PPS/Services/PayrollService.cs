using Microsoft.EntityFrameworkCore;
using PPS.Data;
using PPS.Models;

namespace PPS.Services;

public interface IPayrollService
{
    Task<PayrollRecord> ComputeAsync(int employeeId, int year, int month, PayPeriod period);
    Task<IEnumerable<PayrollRecord>> ComputeAllAsync(int year, int month, PayPeriod period);
    Task<IEnumerable<PayrollRecord>> GetByPeriodAsync(int year, int month, PayPeriod period);
    Task<PayrollRecord?> GetAsync(int employeeId, int year, int month, PayPeriod period);
    Task PostAsync(int payrollId);
    Task<IEnumerable<PayrollRecord>> GetHistoryAsync(int employeeId, int year);
    Task<decimal> GetTotalAnnualBasicSalaryAsync(int employeeId, int year);
    Task<PayrollRecord> Compute13thMonthAsync(int employeeId, int year);
    Task<decimal> GetPeriodTotalNetPayAsync(int year, int month, PayPeriod period);
}

public class PayrollService : IPayrollService
{
    private readonly AppDbContext              _db;
    private readonly PayrollCalculatorService  _calc;
    private readonly IAttendanceService        _attendance;
    private readonly ILoanService              _loans;

    public PayrollService(AppDbContext db, PayrollCalculatorService calc,
        IAttendanceService attendance, ILoanService loans)
    {
        _db         = db;
        _calc       = calc;
        _attendance = attendance;
        _loans      = loans;
    }

    public async Task<PayrollRecord> ComputeAsync(
        int employeeId, int year, int month, PayPeriod period)
    {
        var emp = await _db.Employees.FindAsync(employeeId)
            ?? throw new InvalidOperationException($"Employee {employeeId} not found.");

        var (start, end) = GetPeriodDates(year, month, period);
        var (lateMin, absentDays, otHrs) = await _attendance.GetSummaryAsync(employeeId, start, end);
        decimal loanDed = await _loans.GetTotalSemiMonthlyDeductionAsync(employeeId);

        var input = new PayrollInput
        {
            BasicMonthlySalary = emp.BasicMonthlySalary,
            HourlyRate         = emp.HourlyRate,
            PayrollType        = emp.PayrollType,
            OvertimeHours      = otHrs,
            LateMinutes        = lateMin,
            AbsentDays         = absentDays,
            LoanDeductions     = loanDed
        };

        var r = _calc.Compute(input);
        return await UpsertRecordAsync(new PayrollRecord
        {
            EmployeeId          = employeeId,
            PayYear             = year,
            PayMonth            = month,
            PayPeriod           = period,
            PayPeriodStart      = start,
            PayPeriodEnd        = end,
            BasicPay            = r.BasicPay,
            OvertimePay         = r.OvertimePay,
            GrossPay            = r.GrossPay,
            TardinessDeduction  = r.TardinessDeduction,
            AbsenceDeduction    = r.AbsenceDeduction,
            SSS_Employee        = r.SSS_Employee,
            PhilHealth_Employee = r.PhilHealth_Employee,
            PagIBIG_Employee    = r.PagIBIG_Employee,
            SSS_Employer        = r.SSS_Employer,
            PhilHealth_Employer = r.PhilHealth_Employer,
            PagIBIG_Employer    = r.PagIBIG_Employer,
            TaxableIncome       = r.TaxableIncome,
            WithholdingTax      = r.WithholdingTax,
            LoanDeductions      = r.LoanDeductions,
            NetPay              = r.NetPay,
            GeneratedDate       = DateTime.Now
        });
    }

    public async Task<IEnumerable<PayrollRecord>> ComputeAllAsync(
        int year, int month, PayPeriod period)
    {
        var employees = await _db.Employees.Where(e => e.IsActive).ToListAsync();
        var records   = new List<PayrollRecord>();

        foreach (var emp in employees)
            records.Add(await ComputeAsync(emp.EmployeeId, year, month, period));

        return records;
    }

    public async Task<IEnumerable<PayrollRecord>> GetByPeriodAsync(
        int year, int month, PayPeriod period)
        => await _db.PayrollRecords
            .Include(p => p.Employee)
            .Where(p => p.PayYear == year && p.PayMonth == month
                     && p.PayPeriod == period && !p.Is13thMonth)
            .OrderBy(p => p.Employee!.LastName)
            .ToListAsync();

    public async Task<PayrollRecord?> GetAsync(
        int employeeId, int year, int month, PayPeriod period)
        => await _db.PayrollRecords
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId
                && p.PayYear == year && p.PayMonth == month && p.PayPeriod == period);

    public async Task PostAsync(int payrollId)
    {
        var record = await _db.PayrollRecords.FindAsync(payrollId)
            ?? throw new InvalidOperationException("Payroll record not found.");

        if (record.IsPosted)
            throw new InvalidOperationException("Payroll already posted.");

        record.IsPosted = true;
        await _db.SaveChangesAsync();

        // Process loan payments for this employee
        var activeLoans = await _db.LoanRecords
            .Where(l => l.EmployeeId == record.EmployeeId && l.Status == LoanStatus.Active)
            .ToListAsync();

        if (record.LoanDeductions > 0 && activeLoans.Count > 0)
        {
            // Distribute deduction across active loans proportionally (oldest first)
            decimal remaining = record.LoanDeductions;
            foreach (var loan in activeLoans.OrderBy(l => l.StartDate))
            {
                if (remaining <= 0) break;
                decimal payment = Math.Min(loan.SemiMonthlyAmortization, remaining);
                await _loans.ProcessPaymentAsync(loan.LoanId, payment,
                    DateOnly.FromDateTime(DateTime.Today), record.PayrollId);
                remaining -= payment;
            }
        }
    }

    public async Task<IEnumerable<PayrollRecord>> GetHistoryAsync(int employeeId, int year)
        => await _db.PayrollRecords
            .Where(p => p.EmployeeId == employeeId && p.PayYear == year)
            .OrderBy(p => p.PayMonth).ThenBy(p => p.PayPeriod)
            .ToListAsync();

    public async Task<decimal> GetTotalAnnualBasicSalaryAsync(int employeeId, int year)
        => await _db.PayrollRecords
            .Where(p => p.EmployeeId == employeeId && p.PayYear == year
                     && p.IsPosted && !p.Is13thMonth)
            .SumAsync(p => p.BasicPay);

    public async Task<PayrollRecord> Compute13thMonthAsync(int employeeId, int year)
    {
        decimal totalBasic = await GetTotalAnnualBasicSalaryAsync(employeeId, year);
        decimal pay13th    = _calc.Compute13thMonthPay(totalBasic);

        return await UpsertRecordAsync(new PayrollRecord
        {
            EmployeeId     = employeeId,
            PayYear        = year,
            PayMonth       = 12,
            PayPeriod      = PayPeriod.SecondHalf,
            PayPeriodStart = new DateOnly(year, 12, 1),
            PayPeriodEnd   = new DateOnly(year, 12, 31),
            BasicPay       = pay13th,
            GrossPay       = pay13th,
            NetPay         = pay13th, // 13th month is tax-exempt up to ₱90,000
            Is13thMonth    = true,
            GeneratedDate  = DateTime.Now
        });
    }

    public async Task<decimal> GetPeriodTotalNetPayAsync(int year, int month, PayPeriod period)
        => await _db.PayrollRecords
            .Where(p => p.PayYear == year && p.PayMonth == month && p.PayPeriod == period)
            .SumAsync(p => p.NetPay);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<PayrollRecord> UpsertRecordAsync(PayrollRecord record)
    {
        var existing = await _db.PayrollRecords.FirstOrDefaultAsync(p =>
            p.EmployeeId == record.EmployeeId
            && p.PayYear  == record.PayYear
            && p.PayMonth == record.PayMonth
            && p.PayPeriod == record.PayPeriod
            && p.Is13thMonth == record.Is13thMonth);

        if (existing is null)
        {
            await _db.PayrollRecords.AddAsync(record);
            await _db.SaveChangesAsync();
            return record;
        }

        if (existing.IsPosted)
            throw new InvalidOperationException("Cannot recompute a posted payroll record.");

        existing.BasicPay            = record.BasicPay;
        existing.OvertimePay         = record.OvertimePay;
        existing.GrossPay            = record.GrossPay;
        existing.TardinessDeduction  = record.TardinessDeduction;
        existing.AbsenceDeduction    = record.AbsenceDeduction;
        existing.SSS_Employee        = record.SSS_Employee;
        existing.PhilHealth_Employee = record.PhilHealth_Employee;
        existing.PagIBIG_Employee    = record.PagIBIG_Employee;
        existing.SSS_Employer        = record.SSS_Employer;
        existing.PhilHealth_Employer = record.PhilHealth_Employer;
        existing.PagIBIG_Employer    = record.PagIBIG_Employer;
        existing.TaxableIncome       = record.TaxableIncome;
        existing.WithholdingTax      = record.WithholdingTax;
        existing.LoanDeductions      = record.LoanDeductions;
        existing.NetPay              = record.NetPay;
        existing.GeneratedDate       = DateTime.Now;
        await _db.SaveChangesAsync();
        return existing;
    }

    private static (DateOnly Start, DateOnly End) GetPeriodDates(int year, int month, PayPeriod period)
        => period == PayPeriod.FirstHalf
            ? (new DateOnly(year, month, 1),  new DateOnly(year, month, 15))
            : (new DateOnly(year, month, 16), new DateOnly(year, month, DateTime.DaysInMonth(year, month)));
}
