using Microsoft.EntityFrameworkCore;
using PPS.Data;
using PPS.Models;

namespace PPS.Services;

// ── Interface ─────────────────────────────────────────────────────────────────

public interface ISystemLogService
{
    Task LogAsync(string module, string action,
                  string? details  = null,
                  LogSeverity severity = LogSeverity.Info);

    Task<IEnumerable<SystemLog>> GetByDateRangeAsync(DateTime from, DateTime to);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class SystemLogService : ISystemLogService
{
    private readonly AppDbContext       _db;
    private readonly CurrentUserService _session;

    public SystemLogService(AppDbContext db, CurrentUserService session)
    {
        _db      = db;
        _session = session;
    }

    public async Task LogAsync(string module, string action,
                               string? details  = null,
                               LogSeverity severity = LogSeverity.Info)
    {
        try
        {
            await _db.SystemLogs.AddAsync(new SystemLog
            {
                Timestamp = DateTime.Now,
                Username  = _session.CurrentUser?.Username ?? "system",
                Module    = module,
                Action    = action,
                Details   = details,
                Severity  = severity,
            });
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Never let logging errors crash the app
        }
    }

    public async Task<IEnumerable<SystemLog>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _db.SystemLogs
            .Where(l => l.Timestamp >= from && l.Timestamp <= to)
            .OrderByDescending(l => l.Timestamp)
            .Take(2000)
            .ToListAsync();
}
