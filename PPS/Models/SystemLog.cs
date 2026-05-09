using System.ComponentModel.DataAnnotations;

namespace PPS.Models;

public class SystemLog
{
    [Key]
    public int LogId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    /// <summary>e.g. "Login", "Employees", "Payroll", "Attendance", "Loans", "Reports"</summary>
    [MaxLength(50)]
    public string Module { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Details { get; set; }

    public LogSeverity Severity { get; set; } = LogSeverity.Info;
}
