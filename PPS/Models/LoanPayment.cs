using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PPS.Models;

public class LoanPayment
{
    [Key]
    public int LoanPaymentId { get; set; }

    public int LoanId { get; set; }

    /// <summary>Null for manual payments; set when deducted via payroll run.</summary>
    public int? PayrollId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    public DateOnly PaymentDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceAfter { get; set; }

    [MaxLength(200)]
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey(nameof(LoanId))]
    public LoanRecord? LoanRecord { get; set; }
}
