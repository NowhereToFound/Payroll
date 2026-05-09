using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PPS.Models;

public class LoanRecord
{
    [Key]
    public int LoanId { get; set; }

    public int EmployeeId { get; set; }

    public LoanType LoanType { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LoanAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    /// <summary>Full monthly deduction amount.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyAmortization { get; set; }

    /// <summary>Auto-computed per semi-monthly period (not stored in DB).</summary>
    [NotMapped]
    public decimal SemiMonthlyAmortization => MonthlyAmortization / 2m;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Active;

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    public ICollection<LoanPayment> LoanPayments { get; set; } = [];
}
