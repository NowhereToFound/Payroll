using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PPS.Models;

public class PayrollRecord
{
    [Key]
    public int PayrollId { get; set; }

    public int EmployeeId { get; set; }

    public int PayYear { get; set; }

    public int PayMonth { get; set; }

    public PayPeriod PayPeriod { get; set; }

    public DateOnly PayPeriodStart { get; set; }

    public DateOnly PayPeriodEnd { get; set; }

    // ── Earnings ─────────────────────────────────────────
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicPay { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OvertimePay { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossPay { get; set; }

    // ── Attendance Deductions ─────────────────────────────
    [Column(TypeName = "decimal(18,2)")]
    public decimal TardinessDeduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AbsenceDeduction { get; set; }

    // ── Government Contributions — Employee Share ─────────
    [Column(TypeName = "decimal(18,2)")]
    public decimal SSS_Employee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PhilHealth_Employee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PagIBIG_Employee { get; set; }

    // ── Government Contributions — Employer Share (for reporting) ─
    [Column(TypeName = "decimal(18,2)")]
    public decimal SSS_Employer { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PhilHealth_Employer { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PagIBIG_Employer { get; set; }

    // ── Tax ──────────────────────────────────────────────
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxableIncome { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal WithholdingTax { get; set; }

    // ── Loans ─────────────────────────────────────────────
    [Column(TypeName = "decimal(18,2)")]
    public decimal LoanDeductions { get; set; }

    // ── Net ───────────────────────────────────────────────
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetPay { get; set; }

    public bool IsPosted { get; set; } = false;

    public bool Is13thMonth { get; set; } = false;

    public DateTime GeneratedDate { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }
}
