using Microsoft.EntityFrameworkCore;
using PPS.Data;
using PPS.Models;

namespace PPS.Services;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllActiveAsync();
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByCodeAsync(string code);
    Task<Employee?> GetByBiometricIdAsync(string biometricId);
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeactivateAsync(int id);
    Task<bool> EmployeeCodeExistsAsync(string code, int? excludeId = null);
    Task<int> GetActiveCountAsync();
    Task<int> GetTeachingCountAsync();
    Task<int> GetNonTeachingCountAsync();
}

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Employee>> GetAllActiveAsync()
        => await _context.Employees
            .Where(e => e.IsActive)
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .ToListAsync();

    public async Task<IEnumerable<Employee>> GetAllAsync()
        => await _context.Employees
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .ToListAsync();

    public async Task<Employee?> GetByIdAsync(int id)
        => await _context.Employees.FindAsync(id);

    public async Task<Employee?> GetByCodeAsync(string code)
        => await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == code);

    public async Task<Employee?> GetByBiometricIdAsync(string biometricId)
        => await _context.Employees.FirstOrDefaultAsync(e => e.BiometricId == biometricId);

    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp is not null)
        {
            emp.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> EmployeeCodeExistsAsync(string code, int? excludeId = null)
        => await _context.Employees
            .AnyAsync(e => e.EmployeeCode == code && (excludeId == null || e.EmployeeId != excludeId));

    public async Task<int> GetActiveCountAsync()
        => await _context.Employees.CountAsync(e => e.IsActive);

    public async Task<int> GetTeachingCountAsync()
        => await _context.Employees.CountAsync(e => e.IsActive && e.EmployeeType == EmployeeType.Teaching);

    public async Task<int> GetNonTeachingCountAsync()
        => await _context.Employees.CountAsync(e => e.IsActive && e.EmployeeType == EmployeeType.NonTeaching);
}
