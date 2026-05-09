using PPS.Models;

namespace PPS.Services;

// ─────────────────────────────────────────────────────────────────────────────
// DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Input parameters for payroll computation (semi-monthly).</summary>
public class PayrollInput
{
    public decimal BasicMonthlySalary { get; set; }
    public decimal HourlyRate { get; set; }
    public PayrollType PayrollType { get; set; } = PayrollType.Monthly;

    /// <summary>Actual regular hours worked this semi-monthly period (used for HourlyUnit).</summary>
    public decimal RegularHoursWorked { get; set; } = 88m; // default 8h × 11 days

    public decimal OvertimeHours { get; set; } = 0;
    public decimal LateMinutes { get; set; } = 0;
    public int AbsentDays { get; set; } = 0;
    public decimal LoanDeductions { get; set; } = 0;
}

/// <summary>Full payroll computation result (per semi-monthly period).</summary>
public class PayrollResult
{
    public decimal BasicPay { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal TardinessDeduction { get; set; }
    public decimal AbsenceDeduction { get; set; }
    public decimal GrossPay { get; set; }

    // Employee government shares (semi-monthly)
    public decimal SSS_Employee { get; set; }
    public decimal PhilHealth_Employee { get; set; }
    public decimal PagIBIG_Employee { get; set; }

    // Employer government shares (semi-monthly) — for remittance reports
    public decimal SSS_Employer { get; set; }
    public decimal PhilHealth_Employer { get; set; }
    public decimal PagIBIG_Employer { get; set; }

    public decimal TaxableIncome { get; set; }
    public decimal WithholdingTax { get; set; }
    public decimal LoanDeductions { get; set; }
    public decimal NetPay { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Service
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Philippine 2026 Payroll Calculator using the Step-Down Algorithm.
/// All output figures are per SEMI-MONTHLY period.
/// </summary>
public class PayrollCalculatorService
{
    private const int WorkingDaysPerMonth = 22;
    private const int WorkingHoursPerDay = 8;
    private const decimal OvertimeMultiplier = 1.25m;

    #region SSS 2026 MSC Table — 15% total (4.5% EE / 10.5% ER) per month
    /// <summary>
    /// (MinCompensation inclusive, MaxCompensation inclusive, EmployeeMonthly, EmployerMonthly)
    /// Source: SSS Circular 2024-010 (effective 2025 onwards; unchanged for 2026).
    /// </summary>
    private static readonly (decimal Max, decimal Employee, decimal Employer)[] SssTable =
    [
        (5249.99m,   225.00m,   525.00m),
        (5749.99m,   247.50m,   577.50m),
        (6249.99m,   270.00m,   630.00m),
        (6749.99m,   292.50m,   682.50m),
        (7249.99m,   315.00m,   735.00m),
        (7749.99m,   337.50m,   787.50m),
        (8249.99m,   360.00m,   840.00m),
        (8749.99m,   382.50m,   892.50m),
        (9249.99m,   405.00m,   945.00m),
        (9749.99m,   427.50m,   997.50m),
        (10249.99m,  450.00m,  1050.00m),
        (10749.99m,  472.50m,  1102.50m),
        (11249.99m,  495.00m,  1155.00m),
        (11749.99m,  517.50m,  1207.50m),
        (12249.99m,  540.00m,  1260.00m),
        (12749.99m,  562.50m,  1312.50m),
        (13249.99m,  585.00m,  1365.00m),
        (13749.99m,  607.50m,  1417.50m),
        (14249.99m,  630.00m,  1470.00m),
        (14749.99m,  652.50m,  1522.50m),
        (15249.99m,  675.00m,  1575.00m),
        (15749.99m,  697.50m,  1627.50m),
        (16249.99m,  720.00m,  1680.00m),
        (16749.99m,  742.50m,  1732.50m),
        (17249.99m,  765.00m,  1785.00m),
        (17749.99m,  787.50m,  1837.50m),
        (18249.99m,  810.00m,  1890.00m),
        (18749.99m,  832.50m,  1942.50m),
        (19249.99m,  855.00m,  1995.00m),
        (19749.99m,  877.50m,  2047.50m),
        (20249.99m,  900.00m,  2100.00m),
        (20749.99m,  922.50m,  2152.50m),
        (21249.99m,  945.00m,  2205.00m),
        (21749.99m,  967.50m,  2257.50m),
        (22249.99m,  990.00m,  2310.00m),
        (22749.99m, 1012.50m,  2362.50m),
        (23249.99m, 1035.00m,  2415.00m),
        (23749.99m, 1057.50m,  2467.50m),
        (24249.99m, 1080.00m,  2520.00m),
        (24749.99m, 1102.50m,  2572.50m),
        (25249.99m, 1125.00m,  2625.00m),
        (25749.99m, 1147.50m,  2677.50m),
        (26249.99m, 1170.00m,  2730.00m),
        (26749.99m, 1192.50m,  2782.50m),
        (27249.99m, 1215.00m,  2835.00m),
        (27749.99m, 1237.50m,  2887.50m),
        (28249.99m, 1260.00m,  2940.00m),
        (28749.99m, 1282.50m,  2992.50m),
        (29249.99m, 1305.00m,  3045.00m),
        (29749.99m, 1327.50m,  3097.50m),
        (30249.99m, 1350.00m,  3150.00m),
        (30749.99m, 1372.50m,  3202.50m),
        (31249.99m, 1395.00m,  3255.00m),
        (31749.99m, 1417.50m,  3307.50m),
        (32249.99m, 1440.00m,  3360.00m),
        (32749.99m, 1462.50m,  3412.50m),
        (33249.99m, 1485.00m,  3465.00m),
        (33749.99m, 1507.50m,  3517.50m),
        (34249.99m, 1530.00m,  3570.00m),
        (34749.99m, 1552.50m,  3622.50m),
        (decimal.MaxValue, 1575.00m, 3675.00m), // ≥ 34,750 → MSC 35,000
    ];
    #endregion

    #region BIR TRAIN Semi-Monthly Withholding Tax Table (RR 8-2018, in effect 2023+)
    /// <summary>
    /// (LowerLimit, UpperLimit, BaseTax, ExcessRate).
    /// Limits are the taxable income for a SEMI-MONTHLY period.
    /// </summary>
    private static readonly (decimal Lower, decimal Upper, decimal BaseTax, decimal Rate)[] TaxTable =
    [
        (0m,          10_417.00m,   0m,          0.00m),
        (10_417.01m,  16_666.67m,   0m,          0.20m),
        (16_666.68m,  33_332.83m,   1_250.00m,   0.25m),
        (33_332.84m,  83_332.83m,   5_416.67m,   0.30m),
        (83_332.84m,  333_332.83m,  20_416.67m,  0.32m),
        (333_332.84m, decimal.MaxValue, 100_416.67m, 0.35m),
    ];
    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Executes the full Philippine Step-Down payroll algorithm for one
    /// employee for one semi-monthly period.
    /// </summary>
    public PayrollResult Compute(PayrollInput input)
    {
        var result = new PayrollResult();

        // ── Step 1: Semi-Monthly Basic Pay ────────────────────────────────────
        decimal semiMonthlyBasic = ComputeSemiMonthlyBasicPay(input);
        decimal dailyRate        = input.BasicMonthlySalary / WorkingDaysPerMonth;
        decimal hourlyRate       = dailyRate / WorkingHoursPerDay;

        result.TardinessDeduction = Math.Round((input.LateMinutes / 60m) * hourlyRate, 2);
        result.AbsenceDeduction   = Math.Round(input.AbsentDays  * dailyRate, 2);
        result.OvertimePay        = Math.Round(input.OvertimeHours * hourlyRate * OvertimeMultiplier, 2);

        result.BasicPay = Math.Max(0, semiMonthlyBasic - result.TardinessDeduction - result.AbsenceDeduction);
        result.GrossPay = Math.Max(0, result.BasicPay + result.OvertimePay);

        // ── Step 2: Government Contributions (semi-monthly) ───────────────────
        (result.SSS_Employee,      result.SSS_Employer)      = ComputeSSS(input.BasicMonthlySalary);
        (result.PhilHealth_Employee, result.PhilHealth_Employer) = ComputePhilHealth(input.BasicMonthlySalary);
        (result.PagIBIG_Employee,  result.PagIBIG_Employer)  = ComputePagIBIG(input.BasicMonthlySalary);

        // ── Step 3: Taxable Income ────────────────────────────────────────────
        result.TaxableIncome = Math.Max(0,
            result.GrossPay
            - result.SSS_Employee
            - result.PhilHealth_Employee
            - result.PagIBIG_Employee);

        // ── Step 4: Withholding Tax ───────────────────────────────────────────
        result.WithholdingTax = ComputeWithholdingTax(result.TaxableIncome);

        // ── Step 5: Loan Deductions ───────────────────────────────────────────
        result.LoanDeductions = input.LoanDeductions;

        // ── Step 6: Net Pay ───────────────────────────────────────────────────
        result.NetPay = Math.Max(0,
            result.TaxableIncome
            - result.WithholdingTax
            - result.LoanDeductions);

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Sub-computations (public so ViewModels can preview individual values)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Looks up the 2026 SSS MSC table based on monthly basic salary.
    /// Returns (EmployeeSemiMonthly, EmployerSemiMonthly).
    /// </summary>
    public (decimal Employee, decimal Employer) ComputeSSS(decimal monthlyBasicSalary)
    {
        foreach (var (max, emp, er) in SssTable)
            if (monthlyBasicSalary <= max)
                return (Math.Round(emp / 2m, 2), Math.Round(er / 2m, 2));

        var last = SssTable[^1];
        return (Math.Round(last.Employee / 2m, 2), Math.Round(last.Employer / 2m, 2));
    }

    /// <summary>
    /// PhilHealth 2026: 5% total, shared equally (2.5% EE / 2.5% ER).
    /// Floor: ₱250/month employee | Ceiling: ₱2,500/month employee.
    /// Returns (EmployeeSemiMonthly, EmployerSemiMonthly).
    /// </summary>
    public (decimal Employee, decimal Employer) ComputePhilHealth(decimal monthlyBasicSalary)
    {
        decimal monthlyEEShare = Math.Round(
            Math.Max(250m, Math.Min(2_500m, monthlyBasicSalary * 0.025m)), 2);
        return (Math.Round(monthlyEEShare / 2m, 2), Math.Round(monthlyEEShare / 2m, 2));
    }

    /// <summary>
    /// Pag-IBIG: ₱200/month employee (₱100 semi-monthly) for salary > ₱1,500.
    /// Returns (EmployeeSemiMonthly, EmployerSemiMonthly).
    /// </summary>
    public (decimal Employee, decimal Employer) ComputePagIBIG(decimal monthlyBasicSalary)
    {
        if (monthlyBasicSalary <= 1_500m) return (0m, 0m);
        return (100m, 100m); // ₱200/month ÷ 2
    }

    /// <summary>
    /// BIR TRAIN semi-monthly withholding tax computation.
    /// </summary>
    public decimal ComputeWithholdingTax(decimal taxableIncome)
    {
        foreach (var (lower, upper, baseTax, rate) in TaxTable)
            if (taxableIncome <= upper)
                return Math.Round(baseTax + ((taxableIncome - lower) * rate), 2);

        return 0m;
    }

    /// <summary>
    /// 13th Month Pay = Total basic salary earned in the year ÷ 12.
    /// </summary>
    public decimal Compute13thMonthPay(decimal totalBasicSalaryForYear)
        => Math.Round(totalBasicSalaryForYear / 12m, 2);

    // ─────────────────────────────────────────────────────────────────────────
    private static decimal ComputeSemiMonthlyBasicPay(PayrollInput input)
        => input.PayrollType == PayrollType.HourlyUnit
            ? Math.Round(input.RegularHoursWorked * input.HourlyRate, 2)
            : Math.Round(input.BasicMonthlySalary / 2m, 2);
}
