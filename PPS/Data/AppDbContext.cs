using Microsoft.EntityFrameworkCore;
using PPS.Models;

namespace PPS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<LoanRecord> LoanRecords => Set<LoanRecord>();
    public DbSet<LoanPayment> LoanPayments => Set<LoanPayment>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Employee ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.EmployeeCode).IsUnique();
            e.Property(x => x.BasicMonthlySalary).HasPrecision(18, 2);
            e.Property(x => x.HourlyRate).HasPrecision(18, 2);
        });

        // ── AttendanceRecord ──────────────────────────────────────────────────
        modelBuilder.Entity<AttendanceRecord>(a =>
        {
            a.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();
            a.HasOne(x => x.Employee)
             .WithMany(x => x.AttendanceRecords)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PayrollRecord ─────────────────────────────────────────────────────
        modelBuilder.Entity<PayrollRecord>(p =>
        {
            p.HasIndex(x => new { x.EmployeeId, x.PayYear, x.PayMonth, x.PayPeriod, x.Is13thMonth })
             .IsUnique();
            p.HasOne(x => x.Employee)
             .WithMany(x => x.PayrollRecords)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── LoanRecord ────────────────────────────────────────────────────────
        modelBuilder.Entity<LoanRecord>(l =>
        {
            l.HasOne(x => x.Employee)
             .WithMany(x => x.LoanRecords)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── LoanPayment ───────────────────────────────────────────────────────
        modelBuilder.Entity<LoanPayment>(lp =>
        {
            lp.HasOne(x => x.LoanRecord)
              .WithMany(x => x.LoanPayments)
              .HasForeignKey(x => x.LoanId)
              .OnDelete(DeleteBehavior.Cascade);
        });

        // ── SystemLog ─────────────────────────────────────────────────────────
        modelBuilder.Entity<SystemLog>(s =>
        {
            s.HasIndex(x => x.Timestamp);  // fast date-range queries
            s.HasIndex(x => x.Severity);
        });

        // ── Seed default admin user ───────────────────────────────────────────
        // Password: Admin@1234  (BCrypt hash pre-computed — must be static for EF migrations)
        modelBuilder.Entity<User>().HasData(new User
        {
            UserId       = 1,
            Username     = "admin",
            PasswordHash = "$2a$11$K9Do.8L3kOdFTKTK5v7GKuXGsHyEeZNYWJTl9SWpCn0MIXSwmblWu",
            FullName     = "System Administrator",
            Role         = UserRole.Admin,
            IsActive     = true,
            CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
        });
    }
}
