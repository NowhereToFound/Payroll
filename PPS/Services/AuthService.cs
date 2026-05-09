using Microsoft.EntityFrameworkCore;
using PPS.Data;
using PPS.Models;

namespace PPS.Services;

public interface IAuthService
{
    Task<User?> LoginAsync(string username, string password);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<bool> CreateUserAsync(string username, string password, string fullName, UserRole role);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<bool> ToggleUserActiveAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context) => _context = context;

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        user.LastLoginAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateUserAsync(string username, string password, string fullName, UserRole role)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
            return false;

        await _context.Users.AddAsync(new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            Role = role
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
        => await _context.Users.OrderBy(u => u.FullName).ToListAsync();

    public async Task<bool> ToggleUserActiveAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return false;
        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }
}
