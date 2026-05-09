using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PPS.Models;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    [Required, MaxLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MiddleName { get; set; }

    [NotMapped]
    public string FullName => $"{LastName}, {FirstName}{(string.IsNullOrWhiteSpace(MiddleName) ? "" : " " + MiddleName)}";

    [Required, MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    public EmployeeType EmployeeType { get; set; } = EmployeeType.NonTeaching;

    public PayrollType PayrollType { get; set; } = PayrollType.Monthly;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicMonthlySalary { get; set; }

    /// <summary>Used only when PayrollType is HourlyUnit (part-time / load-based).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal HourlyRate { get; set; }

    [MaxLength(20)]
    public string? SSSNumber { get; set; }

    [MaxLength(20)]
    public string? TINNumber { get; set; }

    [MaxLength(20)]
    public string? PhilHealthNumber { get; set; }

    [MaxLength(20)]
    public string? PagIBIGNumber { get; set; }

    /// <summary>Matches the ID exported in the biometric CSV.</summary>
    [MaxLength(30)]
    public string? BiometricId { get; set; }

    public DateOnly DateHired { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public ICollection<PayrollRecord> PayrollRecords { get; set; } = [];
    public ICollection<LoanRecord> LoanRecords { get; set; } = [];
}
