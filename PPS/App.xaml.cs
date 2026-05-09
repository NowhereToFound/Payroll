using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PPS.Data;
using PPS.Services;
using PPS.ViewModels;
using PPS.Views;
using PPS.Views.Pages;

namespace PPS;

public partial class App : Application
{
    private static IServiceProvider _services = null!;

    /// <summary>Globally resolves DI services — used by code-behind files.</summary>
    public static T GetService<T>() where T : class
        => _services.GetRequiredService<T>();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // ── Configuration ─────────────────────────────────────────────────────
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        // ── DI Container ──────────────────────────────────────────────────────
        var services = new ServiceCollection();

        // EF Core + SQL Server
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // Services
        services.AddSingleton<CurrentUserService>();
        services.AddScoped<PayrollCalculatorService>();
        services.AddScoped<IAuthService,       AuthService>();
        services.AddScoped<IEmployeeService,   EmployeeService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ILoanService,       LoanService>();
        services.AddScoped<IPayrollService,    PayrollService>();
        services.AddScoped<ISystemLogService,  SystemLogService>();
        services.AddSingleton<NavigationService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<EmployeeListViewModel>();
        services.AddTransient<EmployeeDetailViewModel>();
        services.AddTransient<AttendanceViewModel>();
        services.AddTransient<PayrollProcessViewModel>();
        services.AddTransient<PayslipViewModel>();
        services.AddTransient<LoanViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<SystemLogViewModel>();

        // Windows
        services.AddTransient<LoginWindow>();
        services.AddTransient<ShellWindow>();

        _services = services.BuildServiceProvider();

        // ── Ensure database is ready ──────────────────────────────────────────
        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // EnsureCreated: creates DB + tables if they don't exist
            // Returns false (no-op) if DB already exists — safe with schema.sql installs
            db.Database.EnsureCreated();

            // ── Schema patches (idempotent — safe to run every launch) ─────────
            // Adds columns that may be missing on databases created via the original schema.sql
            db.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'LastLoginAt')
                BEGIN
                    ALTER TABLE [dbo].[Users] ADD [LastLoginAt] DATETIME2 NULL
                END;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.objects
                    WHERE object_id = OBJECT_ID(N'[dbo].[SystemLogs]') AND type = 'U')
                BEGIN
                    CREATE TABLE [dbo].[SystemLogs] (
                        [LogId]     INT IDENTITY(1,1) NOT NULL,
                        [Timestamp] DATETIME2 NOT NULL,
                        [Username]  NVARCHAR(100) NOT NULL,
                        [Module]    NVARCHAR(50)  NOT NULL,
                        [Action]    NVARCHAR(250) NOT NULL,
                        [Details]   NVARCHAR(1000) NULL,
                        [Severity]  INT NOT NULL DEFAULT 0,
                        CONSTRAINT [PK_SystemLogs] PRIMARY KEY CLUSTERED ([LogId] ASC)
                    );
                    CREATE NONCLUSTERED INDEX [IX_SystemLogs_Timestamp]
                        ON [dbo].[SystemLogs] ([Timestamp] DESC);
                END;
            ");

            // ── Verify admin password hash is valid ───────────────────────────
            // schema.sql used a placeholder hash — re-compute if it doesn't verify
            var adminUser = db.Users.FirstOrDefault(u => u.Username == "admin");
            if (adminUser != null &&
                !BCrypt.Net.BCrypt.Verify("Admin@1234", adminUser.PasswordHash))
            {
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234");
                db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Database initialization failed:\n\n{ex.Message}\n\n" +
                "Check 'appsettings.json' → ConnectionStrings → DefaultConnection.\n" +
                "Make sure SQL Server is running and the server name is correct.",
                "SDSC PPS — Database Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        // ── Launch Login ──────────────────────────────────────────────────────
        _services.GetRequiredService<LoginWindow>().Show();
    }
}
