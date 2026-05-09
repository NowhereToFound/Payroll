using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PPS.Models;

public class AttendanceRecord
{
    [Key]
    public int AttendanceId { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly? TimeIn { get; set; }

    public TimeOnly? TimeOut { get; set; }

    [Column(TypeName = "decimal(7,2)")]
    public decimal OvertimeHours { get; set; } = 0;

    public LeaveType LeaveType { get; set; } = LeaveType.None;

    public bool IsAbsent { get; set; } = false;

    /// <summary>Minutes past the standard time-in (e.g. 08:00).</summary>
    [Column(TypeName = "decimal(7,2)")]
    public decimal LateMinutes { get; set; } = 0;

    [MaxLength(200)]
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }
}
