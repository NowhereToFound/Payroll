using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using PPS.Data;
using PPS.Models;

namespace PPS.Services;

// ── Biometric CSV mapping ─────────────────────────────────────────────────────

public class BiometricCsvRow
{
    public string EmployeeId { get; set; } = string.Empty;
    public string Date       { get; set; } = string.Empty;
    public string TimeIn     { get; set; } = string.Empty;
    public string TimeOut    { get; set; } = string.Empty;
}

public sealed class BiometricImportMap : ClassMap<BiometricCsvRow>
{
    public BiometricImportMap()
    {
        // Accepts common column name variants exported by biometric devices
        Map(m => m.EmployeeId).Name("ID", "Employee ID", "Emp No", "BiometricID", "EmpID");
        Map(m => m.Date).Name("Date", "Attendance Date", "Work Date");
        Map(m => m.TimeIn).Name("TimeIn", "Time In", "Check In", "IN");
        Map(m => m.TimeOut).Name("TimeOut", "Time Out", "Check Out", "OUT");
    }
}

public class CsvImportResult
{
    public int Imported { get; set; }
    public int Skipped  { get; set; }
    public List<string> Errors { get; set; } = [];
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceRecord>> GetByPeriodAsync(int employeeId, DateOnly start, DateOnly end);
    Task<IEnumerable<AttendanceRecord>> GetAllByPeriodAsync(DateOnly start, DateOnly end);
    Task UpsertAsync(AttendanceRecord record);
    Task<CsvImportResult> ImportFromCsvAsync(string filePath);
    Task<(decimal LateMinutes, int AbsentDays, decimal OvertimeHours)> GetSummaryAsync(
        int employeeId, DateOnly start, DateOnly end);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class AttendanceService : IAttendanceService
{
    private static readonly TimeOnly StandardIn  = new(8, 0);
    private static readonly TimeOnly StandardOut = new(17, 0);

    private readonly AppDbContext     _db;
    private readonly IEmployeeService _employees;

    public AttendanceService(AppDbContext db, IEmployeeService employees)
    {
        _db        = db;
        _employees = employees;
    }

    public async Task<IEnumerable<AttendanceRecord>> GetByPeriodAsync(
        int employeeId, DateOnly start, DateOnly end)
        => await _db.AttendanceRecords
            .Where(a => a.EmployeeId == employeeId && a.Date >= start && a.Date <= end)
            .OrderBy(a => a.Date)
            .ToListAsync();

    public async Task<IEnumerable<AttendanceRecord>> GetAllByPeriodAsync(DateOnly start, DateOnly end)
        => await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.Date >= start && a.Date <= end)
            .OrderBy(a => a.Employee!.LastName).ThenBy(a => a.Date)
            .ToListAsync();

    public async Task UpsertAsync(AttendanceRecord record)
    {
        var existing = await _db.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == record.EmployeeId && a.Date == record.Date);

        if (existing is null)
            await _db.AttendanceRecords.AddAsync(record);
        else
        {
            existing.TimeIn        = record.TimeIn;
            existing.TimeOut       = record.TimeOut;
            existing.LateMinutes   = record.LateMinutes;
            existing.OvertimeHours = record.OvertimeHours;
            existing.LeaveType     = record.LeaveType;
            existing.IsAbsent      = record.IsAbsent;
            existing.Remarks       = record.Remarks;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<CsvImportResult> ImportFromCsvAsync(string filePath)
    {
        var result = new CsvImportResult();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord    = true,
            MissingFieldFound  = null,
            HeaderValidated    = null,
            BadDataFound       = null,
        };

        List<BiometricCsvRow> rows;
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv    = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<BiometricImportMap>();
            rows = csv.GetRecords<BiometricCsvRow>().ToList();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to read CSV: {ex.Message}");
            return result;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var row     = rows[i];
            int lineNum = i + 2; // +2 for header + 1-based

            try
            {
                var employee = await _employees.GetByBiometricIdAsync(row.EmployeeId)
                            ?? await _employees.GetByCodeAsync(row.EmployeeId);

                if (employee is null)
                {
                    result.Errors.Add($"Line {lineNum}: Employee '{row.EmployeeId}' not found.");
                    result.Skipped++;
                    continue;
                }

                if (!DateOnly.TryParse(row.Date, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date))
                {
                    result.Errors.Add($"Line {lineNum}: Invalid date '{row.Date}'.");
                    result.Skipped++;
                    continue;
                }

                TimeOnly? timeIn  = TryParseTime(row.TimeIn);
                TimeOnly? timeOut = TryParseTime(row.TimeOut);

                decimal lateMinutes   = 0m;
                decimal overtimeHours = 0m;

                if (timeIn.HasValue && timeIn.Value > StandardIn)
                    lateMinutes = (decimal)(timeIn.Value - StandardIn).TotalMinutes;

                if (timeOut.HasValue && timeOut.Value > StandardOut)
                    overtimeHours = (decimal)(timeOut.Value - StandardOut).TotalHours;

                await UpsertAsync(new AttendanceRecord
                {
                    EmployeeId     = employee.EmployeeId,
                    Date           = date,
                    TimeIn         = timeIn,
                    TimeOut        = timeOut,
                    LateMinutes    = Math.Round(lateMinutes,   2),
                    OvertimeHours  = Math.Round(overtimeHours, 2),
                    IsAbsent       = !timeIn.HasValue && !timeOut.HasValue,
                });

                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Line {lineNum}: {ex.Message}");
                result.Skipped++;
            }
        }

        return result;
    }

    public async Task<(decimal LateMinutes, int AbsentDays, decimal OvertimeHours)> GetSummaryAsync(
        int employeeId, DateOnly start, DateOnly end)
    {
        var records = await GetByPeriodAsync(employeeId, start, end);
        return (
            records.Sum(r => r.LateMinutes),
            records.Count(r => r.IsAbsent && r.LeaveType == LeaveType.None),
            records.Sum(r => r.OvertimeHours)
        );
    }

    private static TimeOnly? TryParseTime(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return TimeOnly.TryParse(s, out var t) ? t : null;
    }
}
