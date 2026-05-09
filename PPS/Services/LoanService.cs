using Microsoft.EntityFrameworkCore;
using PPS.Data;
using PPS.Models;

namespace PPS.Services;

public interface ILoanService
{
    Task<IEnumerable<LoanRecord>> GetActiveLoansAsync(int employeeId);
    Task<IEnumerable<LoanRecord>> GetAllLoansAsync(int employeeId);
    Task<LoanRecord?> GetByIdAsync(int loanId);
    Task AddLoanAsync(LoanRecord loan);
    Task UpdateLoanAsync(LoanRecord loan);
    Task<decimal> GetTotalSemiMonthlyDeductionAsync(int employeeId);
    Task ProcessPaymentAsync(int loanId, decimal amount, DateOnly paymentDate, int? payrollId = null);
    Task<IEnumerable<LoanRecord>> GetAllActiveLoansAsync();
}

public class LoanService : ILoanService
{
    private readonly AppDbContext _db;

    public LoanService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<LoanRecord>> GetActiveLoansAsync(int employeeId)
        => await _db.LoanRecords
            .Include(l => l.LoanPayments)
            .Where(l => l.EmployeeId == employeeId && l.Status == LoanStatus.Active)
            .OrderBy(l => l.LoanType)
            .ToListAsync();

    public async Task<IEnumerable<LoanRecord>> GetAllLoansAsync(int employeeId)
        => await _db.LoanRecords
            .Include(l => l.LoanPayments)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<LoanRecord?> GetByIdAsync(int loanId)
        => await _db.LoanRecords
            .Include(l => l.LoanPayments)
            .FirstOrDefaultAsync(l => l.LoanId == loanId);

    public async Task AddLoanAsync(LoanRecord loan)
    {
        loan.Balance   = loan.LoanAmount;
        loan.Status    = LoanStatus.Active;
        loan.CreatedAt = DateTime.Now;
        await _db.LoanRecords.AddAsync(loan);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateLoanAsync(LoanRecord loan)
    {
        _db.LoanRecords.Update(loan);
        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalSemiMonthlyDeductionAsync(int employeeId)
    {
        var active = await GetActiveLoansAsync(employeeId);
        return active.Sum(l => l.SemiMonthlyAmortization);
    }

    public async Task ProcessPaymentAsync(int loanId, decimal amount,
        DateOnly paymentDate, int? payrollId = null)
    {
        var loan = await _db.LoanRecords.FindAsync(loanId)
            ?? throw new InvalidOperationException($"Loan {loanId} not found.");

        loan.Balance = Math.Max(0m, loan.Balance - amount);

        if (loan.Balance == 0m)
        {
            loan.Status  = LoanStatus.FullyPaid;
            loan.EndDate = paymentDate;
        }

        await _db.LoanPayments.AddAsync(new LoanPayment
        {
            LoanId       = loanId,
            PayrollId    = payrollId,
            AmountPaid   = amount,
            PaymentDate  = paymentDate,
            BalanceAfter = loan.Balance,
        });

        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<LoanRecord>> GetAllActiveLoansAsync()
        => await _db.LoanRecords
            .Include(l => l.Employee)
            .Where(l => l.Status == LoanStatus.Active)
            .OrderBy(l => l.Employee!.LastName)
            .ToListAsync();
}
